using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.DTO.Partnership;
using wedeli.Models.DTO.Common;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;

namespace wedeli.Service.Service
{
    /// <summary>
    /// Order transfer service for managing order transfers to partner companies
    /// </summary>
    public class OrderTransferService : IOrderTransferService
    {
        private readonly IOrderTransferRepository _transferRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ICompanyPartnershipRepository _partnershipRepository;
        private readonly ITransportCompanyRepository _companyRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderTransferService> _logger;

        public OrderTransferService(
            IOrderTransferRepository transferRepository,
            IOrderRepository orderRepository,
            ICompanyPartnershipRepository partnershipRepository,
            ITransportCompanyRepository companyRepository,
            IVehicleRepository vehicleRepository,
            IMapper mapper,
            ILogger<OrderTransferService> logger)
        {
            _transferRepository = transferRepository;
            _orderRepository = orderRepository;
            _partnershipRepository = partnershipRepository;
            _companyRepository = companyRepository;
            _vehicleRepository = vehicleRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get transfer by ID
        /// </summary>
        public async Task<OrderTransferResponseDto> GetTransferByIdAsync(int transferId)
        {
            try
            {
                var transfer = await _transferRepository.GetByIdAsync(transferId);
                var transferDto = _mapper.Map<OrderTransferResponseDto>(transfer);

                _logger.LogInformation("Retrieved transfer: {TransferId}", transferId);
                return transferDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transfer: {TransferId}", transferId);
                throw;
            }
        }

        /// <summary>
        /// Get transfers by order
        /// </summary>
        public async Task<IEnumerable<OrderTransferResponseDto>> GetTransfersByOrderAsync(int orderId)
        {
            try
            {
                var transfers = await _transferRepository.GetByOrderIdAsync(orderId);
                var transferDtos = _mapper.Map<IEnumerable<OrderTransferResponseDto>>(transfers);

                _logger.LogInformation("Retrieved {Count} transfers for order: {OrderId}", transfers.Count(), orderId);
                return transferDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transfers by order: {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Get company transfers (outgoing or incoming)
        /// </summary>
        public async Task<IEnumerable<OrderTransferResponseDto>> GetCompanyTransfersAsync(int companyId, bool outgoing = true)
        {
            try
            {
                IEnumerable<OrderTransfer> transfers;

                if (outgoing)
                {
                    transfers = await _transferRepository.GetByFromCompanyAsync(companyId);
                }
                else
                {
                    transfers = await _transferRepository.GetByToCompanyAsync(companyId);
                }

                var transferDtos = _mapper.Map<IEnumerable<OrderTransferResponseDto>>(transfers);

                _logger.LogInformation("Retrieved {Count} {Direction} transfers for company: {CompanyId}",
                    transfers.Count(), outgoing ? "outgoing" : "incoming", companyId);
                return transferDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company transfers: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Get pending transfers
        /// </summary>
        public async Task<IEnumerable<OrderTransferResponseDto>> GetPendingTransfersAsync(int companyId)
        {
            try
            {
                // Get pending transfers TO this company (they need to accept)
                var transfers = await _transferRepository.GetByStatusAsync("pending", companyId);
                var filteredTransfers = transfers.Where(t => t.ToCompanyId == companyId);
                var transferDtos = _mapper.Map<IEnumerable<OrderTransferResponseDto>>(filteredTransfers);

                _logger.LogInformation("Retrieved {Count} pending transfers for company: {CompanyId}", 
                    transferDtos.Count(), companyId);
                return transferDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending transfers: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Transfer order to partner company
        /// </summary>
        public async Task<OrderTransferResponseDto> TransferOrderAsync(TransferOrderDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                // Verify order exists
                var order = await _orderRepository.GetByIdAsync(dto.OrderId);
                if (order == null)
                    throw new KeyNotFoundException($"Order not found with ID: {dto.OrderId}");

                // Verify to company exists
                var toCompany = await _companyRepository.GetByIdAsync(dto.ToCompanyId);
                if (toCompany == null)
                    throw new KeyNotFoundException($"Company not found with ID: {dto.ToCompanyId}");

                // Get from company (from order's vehicle's company)
                int fromCompanyId = order.Vehicle?.CompanyId ?? 0;
                if (fromCompanyId <= 0)
                    throw new InvalidOperationException("Cannot determine source company for order");

                // Verify partnership exists
                var partnership = await _partnershipRepository.GetPartnershipAsync(fromCompanyId, dto.ToCompanyId);
                if (partnership == null)
                    throw new InvalidOperationException("No partnership exists between these companies");

                // Create transfer
                var transfer = new OrderTransfer
                {
                    OrderId = dto.OrderId,
                    FromCompanyId = fromCompanyId,
                    ToCompanyId = dto.ToCompanyId,
                    TransferReason = dto.TransferReason,
                    TransferFee = dto.TransferFee ?? 0,
                    CommissionPaid = ((partnership.CommissionRate ?? 0) / 100) * (order.CodAmount ?? 0),
                    TransferStatus = "pending",
                    OriginalVehicleId = order.VehicleId
                };

                var createdTransfer = await _transferRepository.AddAsync(transfer);

                // Increment partnership transferred orders
                await _partnershipRepository.IncrementTransferredOrdersAsync(partnership.PartnershipId);

                var transferDto = _mapper.Map<OrderTransferResponseDto>(createdTransfer);

                _logger.LogInformation("Created transfer: {TransferId} for order: {OrderId} to company: {ToCompanyId}",
                    createdTransfer.TransferId, dto.OrderId, dto.ToCompanyId);
                return transferDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring order: {OrderId}", dto?.OrderId);
                throw;
            }
        }

        /// <summary>
        /// Accept transfer
        /// </summary>
        public async Task<bool> AcceptTransferAsync(int transferId, int? newVehicleId = null)
        {
            try
            {
                var transfer = await _transferRepository.GetByIdAsync(transferId);
                if (transfer == null)
                    throw new KeyNotFoundException($"Transfer not found with ID: {transferId}");

                if (transfer.TransferStatus != "pending")
                    throw new InvalidOperationException("Only pending transfers can be accepted");

                // Verify new vehicle if provided
                if (newVehicleId.HasValue && newVehicleId > 0)
                {
                    var vehicle = await _vehicleRepository.GetByIdAsync(newVehicleId.Value);
                    if (vehicle == null)
                        throw new KeyNotFoundException($"Vehicle not found with ID: {newVehicleId}");

                    if (vehicle.CompanyId != transfer.ToCompanyId)
                        throw new InvalidOperationException("Vehicle does not belong to receiving company");
                }

                var result = await _transferRepository.AcceptTransferAsync(transferId, newVehicleId);

                _logger.LogInformation("Accepted transfer: {TransferId}", transferId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting transfer: {TransferId}", transferId);
                throw;
            }
        }

        /// <summary>
        /// Reject transfer
        /// </summary>
        public async Task<bool> RejectTransferAsync(int transferId, string reason)
        {
            try
            {
                var transfer = await _transferRepository.GetByIdAsync(transferId);
                if (transfer == null)
                    throw new KeyNotFoundException($"Transfer not found with ID: {transferId}");

                if (transfer.TransferStatus != "pending")
                    throw new InvalidOperationException("Only pending transfers can be rejected");

                if (string.IsNullOrEmpty(reason))
                    throw new ArgumentException("Rejection reason is required");

                var result = await _transferRepository.UpdateStatusAsync(transferId, "rejected");

                _logger.LogInformation("Rejected transfer: {TransferId} - Reason: {Reason}", transferId, reason);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting transfer: {TransferId}", transferId);
                throw;
            }
        }
    }
}
