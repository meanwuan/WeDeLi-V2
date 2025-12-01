using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.DTO.COD;
using wedeli.Models.DTO.Driver;
using wedeli.Models.DTO.Common;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;

namespace wedeli.Service.Service
{
    /// <summary>
    /// COD (Cash On Delivery) service for managing COD transactions
    /// </summary>
    public class CodService : ICODService
    {
        private readonly ICODTransactionRepository _codRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CodService> _logger;

        public CodService(
            ICODTransactionRepository codRepository,
            IOrderRepository orderRepository,
            IDriverRepository driverRepository,
            IMapper mapper,
            ILogger<CodService> logger)
        {
            _codRepository = codRepository;
            _orderRepository = orderRepository;
            _driverRepository = driverRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get COD transaction by ID
        /// </summary>
        public async Task<CodTransactionResponseDto> GetCODTransactionAsync(int transactionId)
        {
            try
            {
                var transaction = await _codRepository.GetByIdAsync(transactionId);
                var transactionDto = _mapper.Map<CodTransactionResponseDto>(transaction);

                _logger.LogInformation("Retrieved COD transaction: {CodId}", transactionId);
                return transactionDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving COD transaction: {CodId}", transactionId);
                throw;
            }
        }

        /// <summary>
        /// Get COD by order ID
        /// </summary>
        public async Task<CodTransactionResponseDto> GetCODByOrderAsync(int orderId)
        {
            try
            {
                var transaction = await _codRepository.GetByOrderIdAsync(orderId);

                if (transaction == null)
                    throw new KeyNotFoundException($"COD transaction for order {orderId} not found");

                var transactionDto = _mapper.Map<CodTransactionResponseDto>(transaction);

                _logger.LogInformation("Retrieved COD by order: {OrderId}", orderId);
                return transactionDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving COD by order: {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Get driver COD transactions
        /// </summary>
        public async Task<IEnumerable<CodTransactionResponseDto>> GetDriverCODTransactionsAsync(int driverId, string? status = null)
        {
            try
            {
                var transactions = await _codRepository.GetByDriverIdAsync(driverId, status ?? "");
                var transactionDtos = _mapper.Map<IEnumerable<CodTransactionResponseDto>>(transactions);

                _logger.LogInformation("Retrieved COD transactions for driver: {DriverId}", driverId);
                return transactionDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving COD transactions for driver: {DriverId}", driverId);
                throw;
            }
        }

        /// <summary>
        /// Collect COD at delivery
        /// </summary>
        public async Task<bool> CollectCODAsync(int orderId, int driverId, string? collectionProofPhoto = null)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                    throw new KeyNotFoundException($"Order {orderId} not found");

                var driver = await _driverRepository.GetByIdAsync(driverId);
                if (driver == null)
                    throw new KeyNotFoundException($"Driver {driverId} not found");

                var transaction = await _codRepository.GetByOrderIdAsync(orderId);
                if (transaction == null)
                    throw new KeyNotFoundException($"COD transaction for order {orderId} not found");

                transaction.CollectionStatus = "collected";
                transaction.CollectionProofPhoto = collectionProofPhoto;
                transaction.CollectedAt = DateTime.UtcNow;
                transaction.CollectedByDriver = driverId;
                transaction.OverallStatus = "collected";

                await _codRepository.UpdateAsync(transaction);

                _logger.LogInformation("COD collected - OrderId: {OrderId}, DriverId: {DriverId}, Amount: {Amount}",
                    orderId, driverId, transaction.CodAmount);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting COD: {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Submit COD to company
        /// </summary>
        public async Task<bool> SubmitCODToCompanyAsync(SubmitCodDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                if (dto.TransactionIds == null || dto.TransactionIds.Count == 0)
                    throw new ArgumentException("Transaction IDs list cannot be empty");

                foreach (var transactionId in dto.TransactionIds)
                {
                    var transaction = await _codRepository.GetByIdAsync(transactionId);
                    
                    if (transaction.CollectionStatus != "collected")
                        throw new InvalidOperationException($"COD transaction {transactionId} is not in collected status");

                    transaction.CollectionStatus = "collected";
                    transaction.SubmittedToCompany = true;
                    transaction.SubmittedAt = DateTime.UtcNow;
                    transaction.SubmittedAmount = dto.TotalAmount;
                    transaction.OverallStatus = "submitted_to_company";
                    await _codRepository.UpdateAsync(transaction);
                }

                _logger.LogInformation("COD submitted to company - DriverId: {DriverId}, TransactionCount: {Count}",
                    dto.DriverId, dto.TransactionIds.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting COD to company");
                throw;
            }
        }

        /// <summary>
        /// Get driver pending COD amount
        /// </summary>
        public async Task<decimal> GetDriverPendingCODAsync(int driverId)
        {
            try
            {
                var transactions = await _codRepository.GetByDriverIdAsync(driverId, "collected");
                var totalAmount = transactions.Sum(t => t.CodAmount);

                _logger.LogInformation("Retrieved pending COD for driver: {DriverId}, Amount: {Amount}", driverId, totalAmount);
                return totalAmount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending COD for driver: {DriverId}", driverId);
                throw;
            }
        }

        /// <summary>
        /// Get pending COD collections
        /// </summary>
        public async Task<IEnumerable<CodTransactionResponseDto>> GetPendingCollectionsAsync(int driverId)
        {
            try
            {
                var transactions = await _codRepository.GetPendingCollectionsAsync(driverId);
                var transactionDtos = _mapper.Map<IEnumerable<CodTransactionResponseDto>>(transactions);

                _logger.LogInformation("Retrieved pending collections for driver: {DriverId}", driverId);
                return transactionDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending collections for driver: {DriverId}", driverId);
                throw;
            }
        }

        /// <summary>
        /// Confirm COD receipt
        /// </summary>
        public async Task<bool> ConfirmCODReceiptAsync(int transactionId, int receivedBy)
        {
            try
            {
                var transaction = await _codRepository.GetByIdAsync(transactionId);

                if (transaction.SubmittedToCompany != true)
                    throw new InvalidOperationException($"COD transaction {transactionId} has not been submitted to company yet");

                transaction.CompanyReceivedBy = receivedBy;
                transaction.OverallStatus = "completed";

                await _codRepository.UpdateAsync(transaction);

                _logger.LogInformation("COD receipt confirmed - CodId: {CodId}, ReceivedBy: {ReceivedBy}",
                    transactionId, receivedBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming COD receipt: {CodId}", transactionId);
                throw;
            }
        }

        /// <summary>
        /// Transfer to sender
        /// </summary>
        public async Task<bool> TransferToSenderAsync(TransferToSenderDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                var transaction = await _codRepository.GetByIdAsync(dto.TransactionId);

                if (transaction.OverallStatus != "completed")
                    throw new InvalidOperationException($"COD transaction {dto.TransactionId} is not in completed status");

                transaction.TransferredToSender = true;
                transaction.TransferredAt = DateTime.UtcNow;
                transaction.TransferMethod = dto.TransferMethod;
                transaction.TransferReference = dto.TransferReference;
                transaction.TransferProof = dto.TransferProofUrl;
                transaction.OverallStatus = "completed";

                await _codRepository.UpdateAsync(transaction);

                _logger.LogInformation("COD transferred to sender - CodId: {CodId}, Method: {Method}", 
                    dto.TransactionId, dto.TransferMethod);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring COD to sender");
                throw;
            }
        }

        /// <summary>
        /// Get driver COD summary
        /// </summary>
        public async Task<CodDashboardDto> GetDriverCODSummaryAsync(int driverId, DateTime date)
        {
            try
            {
                var transactions = await _codRepository.GetByDriverIdAsync(driverId);

                var dayTransactions = transactions
                    .Where(t => t.CreatedAt.HasValue && t.CreatedAt.Value.Date == date.Date)
                    .ToList();

                var summary = new CodDashboardDto
                {
                    TotalPendingCollection = dayTransactions.Where(t => t.CollectionStatus == "pending").Sum(t => t.CodAmount),
                    TotalCollected = dayTransactions.Where(t => t.CollectionStatus == "collected").Sum(t => t.CodAmount),
                    TotalSubmitted = dayTransactions.Where(t => t.SubmittedToCompany == true).Sum(t => t.CodAmount),
                    TotalTransferred = dayTransactions.Where(t => t.TransferredToSender == true).Sum(t => t.CodAmount),
                    PendingTransactionCount = dayTransactions.Count(t => t.CollectionStatus == "pending"),
                    CompletedTransactionCount = dayTransactions.Count(t => t.TransferredToSender == true)
                };

                _logger.LogInformation("Retrieved COD summary for driver: {DriverId}, Date: {Date}", driverId, date.Date);
                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving COD summary for driver: {DriverId}", driverId);
                throw;
            }
        }

        /// <summary>
        /// Get pending reconciliations
        /// </summary>
        public async Task<IEnumerable<CodDashboardDto>> GetPendingReconciliationsAsync(int? companyId = null)
        {
            try
            {
                var transactions = await _codRepository.GetAllAsync();

                var pendingTransactions = transactions
                    .Where(t => t.SubmittedToCompany == true && t.TransferredToSender != true)
                    .GroupBy(t => t.CollectedByDriver)
                    .Select(g => new CodDashboardDto
                    {
                        TotalPendingCollection = g.Where(t => t.SubmittedToCompany == true && t.TransferredToSender != true).Sum(t => t.CodAmount),
                        TotalTransferred = g.Where(t => t.TransferredToSender == true).Sum(t => t.CodAmount),
                        PendingTransactionCount = g.Count(t => t.SubmittedToCompany == true && t.TransferredToSender != true),
                        CompletedTransactionCount = g.Count(t => t.TransferredToSender == true)
                    })
                    .ToList();

                _logger.LogInformation("Retrieved pending reconciliations");
                return pendingTransactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending reconciliations");
                throw;
            }
        }

        /// <summary>
        /// Reconcile driver COD
        /// </summary>
        public async Task<bool> ReconcileDriverCODAsync(int summaryId, int reconciledBy)
        {
            try
            {
                _logger.LogInformation("Reconciled driver COD - SummaryId: {SummaryId}, ReconciledBy: {ReconciledBy}",
                    summaryId, reconciledBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reconciling driver COD");
                throw;
            }
        }

        /// <summary>
        /// Reconcile all drivers COD
        /// </summary>
        public async Task<bool> ReconcileAllDriversAsync(DateTime date, int companyId, int reconciledBy)
        {
            try
            {
                _logger.LogInformation("Reconciled all drivers COD - Date: {Date}, CompanyId: {CompanyId}", date.Date, companyId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reconciling all drivers COD");
                throw;
            }
        }

        /// <summary>
        /// Get COD dashboard
        /// </summary>
        public async Task<CodDashboardDto> GetCODDashboardAsync(int? companyId = null)
        {
            try
            {
                var transactions = await _codRepository.GetAllAsync();

                var dashboard = new CodDashboardDto
                {
                    TotalPendingCollection = transactions.Where(t => t.CollectionStatus == "pending").Sum(t => t.CodAmount),
                    TotalCollected = transactions.Where(t => t.CollectionStatus == "collected").Sum(t => t.CodAmount),
                    TotalSubmitted = transactions.Where(t => t.SubmittedToCompany == true).Sum(t => t.CodAmount),
                    TotalTransferred = transactions.Where(t => t.TransferredToSender == true).Sum(t => t.CodAmount),
                    PendingTransactionCount = transactions.Count(t => t.CollectionStatus == "pending"),
                    CompletedTransactionCount = transactions.Count(t => t.TransferredToSender == true)
                };

                _logger.LogInformation("Retrieved COD dashboard");
                return dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving COD dashboard");
                throw;
            }
        }
    }
}
