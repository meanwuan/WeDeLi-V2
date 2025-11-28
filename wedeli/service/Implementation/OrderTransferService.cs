using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.DTO;
using wedeli.Repositories.Interface;
using wedeli.service.Interface;

namespace WeDeLi.Services.Implementation
{
    public class OrderTransferService : IOrderTransferService
    {
        private readonly IOrderTransferRepository _transferRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IPartnershipRepository _partnershipRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ILogger<OrderTransferService> _logger;

        public OrderTransferService(
            IOrderTransferRepository transferRepository,
            IOrderRepository orderRepository,
            IPartnershipRepository partnershipRepository,
            IVehicleRepository vehicleRepository,
            ILogger<OrderTransferService> logger)
        {
            _transferRepository = transferRepository;
            _orderRepository = orderRepository;
            _partnershipRepository = partnershipRepository;
            _vehicleRepository = vehicleRepository;
            _logger = logger;
        }

        public async Task<OrderTransferDto> CreateTransferAsync(CreateOrderTransferDto dto, int userId)
        {
            try
            {
                // Validate order exists and is transferable
                var order = await _orderRepository.GetByIdAsync(dto.OrderId);
                if (order == null)
                {
                    throw new KeyNotFoundException($"Order not found: {dto.OrderId}");
                }

                // Check order status is transferable
                var transferableStatuses = new[] { "pending_pickup", "picked_up" };
                if (!transferableStatuses.Contains(order.OrderStatus ?? ""))
                {
                    throw new InvalidOperationException($"Order {dto.OrderId} cannot be transferred. Current status: {order.OrderStatus}");
                }

                if (!order.RouteId.HasValue)
                {
                    throw new InvalidOperationException($"Order {dto.OrderId} does not have a route and cannot determine the source company.");
                }

                // Get partnership to calculate commission
                var partnership = await _partnershipRepository.GetPartnershipAsync(
                    order.Route.CompanyId, // Assuming Route is loaded with the Order
                    dto.ToCompanyId);

                if (partnership == null)
                {
                    throw new InvalidOperationException($"No partnership exists between company {order.Route.CompanyId} and {dto.ToCompanyId}");
                }

                if (partnership.IsActive != true)
                {
                    throw new InvalidOperationException("Partnership is not active");
                }

                // Calculate commission
                var shippingFee = order.ShippingFee;
                var commissionRate = partnership.CommissionRate ?? 0;
                var commission = shippingFee * (commissionRate / 100);

                // Validate target company has available vehicles (optional)
                // We use the order's weight to check for suitable vehicles.
                if (order.WeightKg.HasValue && order.WeightKg > 0)
                {
                    var availableVehicles = await _vehicleRepository.GetAvailableVehiclesForWeightAsync(order.WeightKg.Value, dto.ToCompanyId);

                    if (!availableVehicles.Any())
                    {
                        throw new InvalidOperationException($"Partner company {dto.ToCompanyId} has no available vehicles that can accommodate the order weight of {order.WeightKg}kg.");
                    }
                }

                // Create transfer record
                var transfer = new OrderTransfer
                {
                    OrderId = dto.OrderId,
                    FromCompanyId = order.Route.CompanyId,
                    ToCompanyId = dto.ToCompanyId,
                    TransferReason = dto.TransferReason,
                    OriginalVehicleId = dto.OriginalVehicleId,
                    TransferredBy = userId,
                    TransferFee = shippingFee,
                    CommissionPaid = commission,
                    AdminNotes = dto.AdminNotes,
                    TransferStatus = "pending"
                };

                var createdTransfer = await _transferRepository.CreateTransferAsync(transfer);

                _logger.LogInformation($"Transfer created: Order {dto.OrderId} -> Company {dto.ToCompanyId}, Commission: {commission:N2}");

                return MapToDto(createdTransfer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating transfer for order {dto.OrderId}");
                throw;
            }
        }

        public async Task<List<OrderTransferDto>> GetCompanyTransfersAsync(int companyId, string? status)
        {
            try
            {
                var transfers = await _transferRepository.GetCompanyTransfersAsync(companyId, status);
                return transfers.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting transfers for company {companyId}");
                throw;
            }
        }

        public async Task<OrderTransferDto> AcceptTransferAsync(int transferId, int userId, int? vehicleId)
        {
            try
            {
                // Validate vehicle if provided
                if (vehicleId.HasValue)
                {
                    var vehicle = await _vehicleRepository.GetVehicleByIdAsync(vehicleId.Value);
                    if (vehicle == null)
                    {
                        throw new KeyNotFoundException($"Vehicle not found: {vehicleId}");
                    }

                    if (vehicle.CurrentStatus != "available")
                    {
                        throw new InvalidOperationException($"Vehicle {vehicleId} is not available. Current status: {vehicle.CurrentStatus}");
                    }
                }

                var success = await _transferRepository.AcceptTransferAsync(transferId, userId, vehicleId);
                if (!success)
                {
                    throw new InvalidOperationException($"Failed to accept transfer {transferId}");
                }

                var transfer = await _transferRepository.GetTransferByIdAsync(transferId);
                if (transfer == null)
                {
                    throw new KeyNotFoundException($"Transfer not found after acceptance: {transferId}");
                }

                _logger.LogInformation($"Transfer {transferId} accepted by user {userId}");

                return MapToDto(transfer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error accepting transfer {transferId}");
                throw;
            }
        }

        public async Task<bool> RejectTransferAsync(int transferId, int userId, string reason)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reason))
                {
                    throw new ArgumentException("Rejection reason is required");
                }

                var success = await _transferRepository.RejectTransferAsync(transferId, reason);

                if (success)
                {
                    _logger.LogInformation($"Transfer {transferId} rejected by user {userId}. Reason: {reason}");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rejecting transfer {transferId}");
                throw;
            }
        }

        public async Task<List<OrderTransferDto>> GetPendingTransfersAsync(int companyId)
        {
            try
            {
                var transfers = await _transferRepository.GetPendingTransfersForCompanyAsync(companyId);
                return transfers.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting pending transfers for company {companyId}");
                throw;
            }
        }

        public async Task<OrderTransferDto> GetTransferByIdAsync(int transferId)
        {
            try
            {
                var transfer = await _transferRepository.GetTransferByIdAsync(transferId);
                if (transfer == null)
                {
                    throw new KeyNotFoundException($"Transfer not found: {transferId}");
                }

                return MapToDto(transfer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting transfer {transferId}");
                throw;
            }
        }

        private OrderTransferDto MapToDto(OrderTransfer transfer)
        {
            return new OrderTransferDto
            {
                TransferId = transfer.TransferId,
                OrderId = transfer.OrderId,
                FromCompanyId = transfer.FromCompanyId ,
                FromCompanyName = transfer.FromCompany?.CompanyName,
                ToCompanyId = transfer.ToCompanyId,
                ToCompanyName = transfer.ToCompany?.CompanyName,
                TransferReason = transfer.TransferReason,
                OriginalVehicleId = transfer.OriginalVehicleId,
                NewVehicleId = transfer.NewVehicleId,
                TransferredBy = transfer.TransferredBy,
                TransferredByName = transfer.TransferredByNavigation?.FullName,
                TransferFee = transfer.TransferFee ?? 0,
                CommissionPaid = transfer.CommissionPaid ?? 0,
                AdminNotes = transfer.AdminNotes,
                TransferStatus = transfer.TransferStatus,
                TransferredAt = transfer.TransferredAt ?? DateTime.Now,
                AcceptedAt = transfer.AcceptedAt
            };
        }
    }
}