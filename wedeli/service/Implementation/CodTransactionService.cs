using wedeli.Models.Domain;
using wedeli.Models.DTO;
using wedeli.Repositories.Interface;
using wedeli.service.Interface;

namespace wedeli.service.Implementation
{
    /// <summary>
    /// Service implementation for COD (Cash On Delivery) management
    /// </summary>
    public class CodTransactionService : ICodTransactionService
    {
        private readonly ICodTransactionRepository _codRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly ILogger<CodTransactionService> _logger;

        public CodTransactionService(
            ICodTransactionRepository codRepository,
            IOrderRepository orderRepository,
            IDriverRepository driverRepository,
            ILogger<CodTransactionService> logger)
        {
            _codRepository = codRepository;
            _orderRepository = orderRepository;
            _driverRepository = driverRepository;
            _logger = logger;
        }

        // ===== CRUD Operations =====

        public async Task<CodTransactionDto?> GetCodTransactionByIdAsync(int codTransactionId)
        {
            var cod = await _codRepository.GetCodTransactionByIdAsync(codTransactionId);
            return cod == null ? null : MapToDto(cod);
        }

        public async Task<CodTransactionDto?> GetCodTransactionByOrderIdAsync(int orderId)
        {
            var cod = await _codRepository.GetCodTransactionByOrderIdAsync(orderId);
            return cod == null ? null : MapToDto(cod);
        }

        public async Task<(List<CodTransactionDto> Transactions, int TotalCount)> GetCodTransactionsAsync(CodTransactionFilterDto filter)
        {
            var (cods, totalCount) = await _codRepository.GetCodTransactionsAsync(filter);
            var codDtos = cods.Select(MapToDto).ToList();
            return (codDtos, totalCount);
        }

        public async Task<CodTransactionDto> CreateCodTransactionAsync(CreateCodTransactionDto dto)
        {
            var order = await _orderRepository.GetByIdAsync(dto.OrderId);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order not found: {dto.OrderId}");
            }

            if (!order.CodAmount.HasValue || order.CodAmount == 0)
            {
                throw new InvalidOperationException($"Order {dto.OrderId} does not have a COD amount");
            }

            var cod = new CodTransaction
            {
                OrderId = dto.OrderId,
                CodAmount = dto.CodAmount,
                Notes = dto.Notes,
                OverallStatus = "pending_collection",
                CollectionStatus = "pending"
            };

            var createdCod = await _codRepository.Create(cod);
            _logger.LogInformation($"COD transaction created for order {dto.OrderId}: {createdCod.CodTransactionId}");

            return MapToDto(createdCod);
        }

        // ===== Driver Operations =====

        public async Task<DriverCodResponseDto> GetDriverPendingCodAsync(int driverId)
        {
            var driver = await _driverRepository.GetDriverByIdAsync(driverId);
            if (driver == null)
            {
                throw new KeyNotFoundException($"Driver not found: {driverId}");
            }

            var pendingCods = await _codRepository.GetDriverPendingCodAsync(driverId);
            var totalAmount = await _codRepository.GetDriverPendingCodAmountAsync(driverId);

            return new DriverCodResponseDto
            {
                DriverId = driverId,
                DriverName = driver.User?.FullName ?? "Unknown",
                TotalPendingCod = totalAmount,
                PendingCodCount = pendingCods.Count,
                PendingTransactions = pendingCods.Select(MapToDto).ToList()
            };
        }

        public async Task<CodTransactionDto> RecordCodCollectionAsync(int codTransactionId, string? proofPhotoUrl = null)
        {
            var cod = await _codRepository.RecordCodCollectionAsync(codTransactionId, proofPhotoUrl);
            _logger.LogInformation($"COD collection recorded: {codTransactionId}");
            return MapToDto(cod);
        }

        public async Task<CodTransactionDto> SubmitCodToCompanyAsync(int driverId, List<int> codTransactionIds)
        {
            if (!codTransactionIds.Any())
            {
                throw new ArgumentException("No COD transactions to submit");
            }

            var driver = await _driverRepository.GetDriverByIdAsync(driverId);
            if (driver == null)
            {
                throw new KeyNotFoundException($"Driver not found: {driverId}");
            }

            var cod = await _codRepository.SubmitCodToCompanyAsync(driverId, codTransactionIds);
            _logger.LogInformation($"Driver {driverId} submitted {codTransactionIds.Count} COD transactions");
            return MapToDto(cod);
        }

        // ===== Company Operations =====

        public async Task<List<CodDailySummaryDto>> GetCompanyPendingCodAsync(int companyId)
        {
            var pendingCods = await _codRepository.GetCompanyPendingCodAsync(companyId);
            
            var summary = pendingCods
                .GroupBy(c => c.CollectedByDriver ?? 0)
                .Select(g =>
                {
                    var driver = g.First().CollectedByDriverNavigation;
                    return new CodDailySummaryDto
                    {
                        DriverId = g.Key,
                        DriverName = driver?.User?.FullName ?? "Unknown",
                        Date = DateTime.Now,
                        TotalCodCollected = g.Count(),
                        TotalAmount = g.Sum(c => c.CodAmount),
                        TotalSubmitted = g.Sum(c => c.SubmittedAmount ?? 0),
                        Transactions = g.Select(MapToDto).ToList()
                    };
                })
                .ToList();

            return summary;
        }

        public async Task<CompanyCodReconciliationDto> GetCompanyCodReconciliationAsync(int companyId, DateTime date)
        {
            var (transactions, totalAmount) = await _codRepository.GetCompanyCodSummaryByDateAsync(companyId, date);
            var driverSummary = await _codRepository.GetDailyCodSummaryByDriverAsync(companyId, date);

            var totalSubmitted = transactions.Sum(c => c.SubmittedAmount ?? 0);
            var variance = totalAmount - totalSubmitted;

            var driverSummaries = driverSummary.Select(kvp =>
            {
                var driver = transactions.FirstOrDefault(c => c.CollectedByDriver == kvp.Key)?.CollectedByDriverNavigation;
                return new CodDailySummaryDto
                {
                    DriverId = kvp.Key,
                    DriverName = driver?.User?.FullName ?? "Unknown",
                    Date = date,
                    TotalCodCollected = transactions.Count(c => c.CollectedByDriver == kvp.Key),
                    TotalAmount = kvp.Value,
                    Transactions = transactions.Where(c => c.CollectedByDriver == kvp.Key).Select(MapToDto).ToList()
                };
            }).ToList();

            return new CompanyCodReconciliationDto
            {
                CompanyId = companyId,
                CompanyName = "Company", // TODO: Get actual company name
                Date = date,
                TotalCollected = totalAmount,
                TotalSubmittedByDrivers = totalSubmitted,
                Variance = variance,
                TransactionCount = transactions.Count,
                DriverSummaries = driverSummaries
            };
        }

        public async Task<CodTransactionDto> ReceiveCodFromDriverAsync(int codTransactionId, int receivedByUserId)
        {
            var cod = await _codRepository.ReceiveCodFromDriverAsync(codTransactionId, receivedByUserId);
            _logger.LogInformation($"COD received from driver: {codTransactionId}");
            return MapToDto(cod);
        }

        public async Task<CodTransactionDto> TransferCodToSenderAsync(int codTransactionId, string transferMethod, string? transferReference = null, string? transferProofUrl = null)
        {
            var cod = await _codRepository.TransferCodToSenderAsync(codTransactionId, transferMethod, transferReference, transferProofUrl);
            _logger.LogInformation($"COD transferred to sender: {codTransactionId}, Method: {transferMethod}");
            return MapToDto(cod);
        }

        // ===== Helper Methods =====

        private CodTransactionDto MapToDto(CodTransaction cod)
        {
            return new CodTransactionDto
            {
                CodTransactionId = cod.CodTransactionId,
                OrderId = cod.OrderId,
                TrackingCode = cod.Order?.TrackingCode ?? "Unknown",
                CodAmount = cod.CodAmount,
                CollectedByDriver = cod.CollectedByDriver,
                DriverName = cod.CollectedByDriverNavigation?.User?.FullName,
                CollectedAt = cod.CollectedAt,
                CollectionStatus = cod.CollectionStatus,
                CollectionProofPhoto = cod.CollectionProofPhoto,
                SubmittedToCompany = cod.SubmittedToCompany,
                SubmittedAt = cod.SubmittedAt,
                SubmittedAmount = cod.SubmittedAmount,
                CompanyReceivedBy = cod.CompanyReceivedBy,
                TransferredToSender = cod.TransferredToSender,
                TransferredAt = cod.TransferredAt,
                TransferMethod = cod.TransferMethod,
                TransferReference = cod.TransferReference,
                CompanyFee = cod.CompanyFee,
                AdjustmentAmount = cod.AdjustmentAmount,
                AdjustmentReason = cod.AdjustmentReason,
                OverallStatus = cod.OverallStatus,
                Notes = cod.Notes,
                CreatedAt = cod.CreatedAt,
                UpdatedAt = cod.UpdatedAt
            };
        }
    }
}
