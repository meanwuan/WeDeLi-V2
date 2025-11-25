using Microsoft.EntityFrameworkCore;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Models.DTO;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Repo
{
    public class CodTransactionRepository : ICodTransactionRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CodTransactionRepository> _logger;

        public CodTransactionRepository(AppDbContext context, ILogger<CodTransactionRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ===== CRUD Operations =====

        public async Task<CodTransaction?> GetCodTransactionByIdAsync(int codTransactionId)
        {
            try
            {
                return await _context.CodTransactions
                    .Include(c => c.Order)
                    .Include(c => c.CollectedByDriverNavigation)
                    .Include(c => c.CompanyReceivedByNavigation)
                    .FirstOrDefaultAsync(c => c.CodTransactionId == codTransactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting COD transaction: {codTransactionId}");
                throw;
            }
        }

        public async Task<CodTransaction?> GetCodTransactionByOrderIdAsync(int orderId)
        {
            try
            {
                return await _context.CodTransactions
                    .Include(c => c.Order)
                    .FirstOrDefaultAsync(c => c.OrderId == orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting COD transaction by order: {orderId}");
                throw;
            }
        }

        public async Task<(List<CodTransaction> Transactions, int TotalCount)> GetCodTransactionsAsync(CodTransactionFilterDto filter)
        {
            try
            {
                var query = _context.CodTransactions
                    .Include(c => c.Order)
                    .Include(c => c.CollectedByDriverNavigation)
                    .Include(c => c.CompanyReceivedByNavigation)
                    .AsQueryable();

                if (filter.CompanyId.HasValue)
                {
                    // Get CompanyId through Vehicle -> TransportCompany relationship
                    var companyDriverIds = await _context.Drivers
                        .Where(d => d.CompanyId == filter.CompanyId.Value)
                        .Select(d => d.DriverId)
                        .ToListAsync();
                    query = query.Where(c => companyDriverIds.Contains(c.CollectedByDriver ?? 0));
                }

                if (filter.DriverId.HasValue)
                {
                    query = query.Where(c => c.CollectedByDriver == filter.DriverId.Value);
                }

                if (!string.IsNullOrEmpty(filter.CollectionStatus))
                {
                    query = query.Where(c => c.CollectionStatus == filter.CollectionStatus);
                }

                if (!string.IsNullOrEmpty(filter.OverallStatus))
                {
                    query = query.Where(c => c.OverallStatus == filter.OverallStatus);
                }

                if (filter.StartDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt <= filter.EndDate.Value);
                }

                var totalCount = await query.CountAsync();

                var transactions = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                return (transactions, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting COD transactions");
                throw;
            }
        }

        // ===== Driver Operations =====

        public async Task<List<CodTransaction>> GetDriverPendingCodAsync(int driverId)
        {
            try
            {
                return await _context.CodTransactions
                    .Include(c => c.Order)
                    .Where(c => c.CollectedByDriver == driverId 
                           && c.CollectionStatus == "collected" 
                           && c.SubmittedToCompany != true)
                    .OrderBy(c => c.CollectedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting driver pending COD: {driverId}");
                throw;
            }
        }

        public async Task<decimal> GetDriverPendingCodAmountAsync(int driverId)
        {
            try
            {
                return await _context.CodTransactions
                    .Where(c => c.CollectedByDriver == driverId 
                           && c.CollectionStatus == "collected" 
                           && c.SubmittedToCompany != true)
                    .SumAsync(c => c.CodAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating driver pending COD amount: {driverId}");
                throw;
            }
        }

        public async Task<CodTransaction> RecordCodCollectionAsync(int codTransactionId, string? proofPhotoUrl = null)
        {
            try
            {
                var codTransaction = await _context.CodTransactions.FindAsync(codTransactionId);
                if (codTransaction == null)
                    throw new KeyNotFoundException($"COD transaction not found: {codTransactionId}");

                codTransaction.CollectionStatus = "collected";
                codTransaction.CollectedAt = DateTime.Now;
                codTransaction.CollectionProofPhoto = proofPhotoUrl;
                codTransaction.OverallStatus = "collected";
                codTransaction.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"COD collection recorded: {codTransactionId}");
                return codTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error recording COD collection: {codTransactionId}");
                throw;
            }
        }

        public async Task<CodTransaction> SubmitCodToCompanyAsync(int driverId, List<int> codTransactionIds)
        {
            try
            {
                var codTransactions = await _context.CodTransactions
                    .Where(c => codTransactionIds.Contains(c.CodTransactionId) 
                           && c.CollectedByDriver == driverId)
                    .ToListAsync();

                if (!codTransactions.Any())
                    throw new InvalidOperationException("No COD transactions found for this driver");

                foreach (var cod in codTransactions)
                {
                    cod.SubmittedToCompany = true;
                    cod.SubmittedAt = DateTime.Now;
                    cod.SubmittedAmount = cod.CodAmount;
                    cod.OverallStatus = "submitted_to_company";
                    cod.UpdatedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"COD submitted to company by driver {driverId}: {codTransactions.Count} transactions");
                return codTransactions.First();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error submitting COD to company: Driver {driverId}");
                throw;
            }
        }

        // ===== Company Operations =====

        public async Task<List<CodTransaction>> GetCompanyPendingCodAsync(int companyId)
        {
            try
            {
                var companyDriverIds = await _context.Drivers
                    .Where(d => d.CompanyId == companyId)
                    .Select(d => d.DriverId)
                    .ToListAsync();

                return await _context.CodTransactions
                    .Include(c => c.Order)
                    .Include(c => c.CollectedByDriverNavigation)
                    .Where(c => companyDriverIds.Contains(c.CollectedByDriver ?? 0) 
                           && c.SubmittedToCompany == true 
                           && c.TransferredToSender != true)
                    .OrderBy(c => c.SubmittedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting company pending COD: {companyId}");
                throw;
            }
        }

        public async Task<(List<CodTransaction> Transactions, decimal TotalAmount)> GetCompanyCodSummaryByDateAsync(int companyId, DateTime date)
        {
            try
            {
                var dateOnly = DateOnly.FromDateTime(date);

                var companyDriverIds = await _context.Drivers
                    .Where(d => d.CompanyId == companyId)
                    .Select(d => d.DriverId)
                    .ToListAsync();

                var transactions = await _context.CodTransactions
                    .Include(c => c.Order)
                    .Include(c => c.CollectedByDriverNavigation)
                    .Where(c => companyDriverIds.Contains(c.CollectedByDriver ?? 0) 
                           && c.CollectedAt.HasValue 
                           && DateOnly.FromDateTime(c.CollectedAt.Value) == dateOnly)
                    .ToListAsync();

                var totalAmount = transactions.Sum(c => c.CodAmount);

                return (transactions, totalAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting company COD summary: {companyId}, {date}");
                throw;
            }
        }

        public async Task<CodTransaction> ReceiveCodFromDriverAsync(int codTransactionId, int receivedByUserId)
        {
            try
            {
                var codTransaction = await _context.CodTransactions.FindAsync(codTransactionId);
                if (codTransaction == null)
                    throw new KeyNotFoundException($"COD transaction not found: {codTransactionId}");

                codTransaction.CompanyReceivedBy = receivedByUserId;
                codTransaction.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"COD received by company: {codTransactionId}");
                return codTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error receiving COD from driver: {codTransactionId}");
                throw;
            }
        }

        public async Task<CodTransaction> TransferCodToSenderAsync(int codTransactionId, string transferMethod, string? transferReference = null, string? transferProofUrl = null)
        {
            try
            {
                var codTransaction = await _context.CodTransactions.FindAsync(codTransactionId);
                if (codTransaction == null)
                    throw new KeyNotFoundException($"COD transaction not found: {codTransactionId}");

                codTransaction.TransferredToSender = true;
                codTransaction.TransferredAt = DateTime.Now;
                codTransaction.TransferMethod = transferMethod;
                codTransaction.TransferReference = transferReference;
                codTransaction.TransferProof = transferProofUrl;
                codTransaction.OverallStatus = "completed";
                codTransaction.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"COD transferred to sender: {codTransactionId}");
                return codTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error transferring COD to sender: {codTransactionId}");
                throw;
            }
        }

        // ===== Reconciliation =====

        public async Task<List<CodTransaction>> ReconcileDriverCodAsync(int driverId)
        {
            try
            {
                var codTransactions = await _context.CodTransactions
                    .Where(c => c.CollectedByDriver == driverId 
                           && c.CollectionStatus == "collected" 
                           && c.SubmittedToCompany != true)
                    .ToListAsync();

                return codTransactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reconciling driver COD: {driverId}");
                throw;
            }
        }

        public async Task<Dictionary<int, decimal>> GetDailyCodSummaryByDriverAsync(int companyId, DateTime date)
        {
            try
            {
                var dateOnly = DateOnly.FromDateTime(date);

                var companyDriverIds = await _context.Drivers
                    .Where(d => d.CompanyId == companyId)
                    .Select(d => d.DriverId)
                    .ToListAsync();

                var summary = await _context.CodTransactions
                    .Include(c => c.Order)
                    .Where(c => companyDriverIds.Contains(c.CollectedByDriver ?? 0) 
                           && c.CollectedAt.HasValue 
                           && DateOnly.FromDateTime(c.CollectedAt.Value) == dateOnly)
                    .GroupBy(c => c.CollectedByDriver ?? 0)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Sum(c => c.CodAmount)
                    );

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting daily COD summary by driver: {companyId}, {date}");
                throw;
            }
        }

        public async Task<decimal> CalculateCompanyFeeAsync(decimal codAmount, int companyId)
        {
            try
            {
                // TODO: Implement fee calculation logic based on company settings
                // For now, assuming a flat 2% fee
                var feePercentage = 0.02m;
                return codAmount * feePercentage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating company fee");
                throw;
            }
        }

        // ===== Query & Reports =====

        public async Task<List<CodTransaction>> GetCodTransactionsByStatusAsync(string status, int? companyId = null)
        {
            try
            {
                var query = _context.CodTransactions
                    .Include(c => c.Order)
                    .Where(c => c.OverallStatus == status);

                if (companyId.HasValue)
                {
                    var companyDriverIds = await _context.Drivers
                        .Where(d => d.CompanyId == companyId.Value)
                        .Select(d => d.DriverId)
                        .ToListAsync();
                    query = query.Where(c => companyDriverIds.Contains(c.CollectedByDriver ?? 0));
                }

                return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting COD transactions by status: {status}");
                throw;
            }
        }

        public async Task<CodTransaction> Create(CodTransaction codTransaction)
        {
            try
            {
                codTransaction.CreatedAt = DateTime.Now;
                codTransaction.OverallStatus = "pending_collection";
                codTransaction.CollectionStatus = "pending";

                await _context.CodTransactions.AddAsync(codTransaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"COD transaction created: {codTransaction.CodTransactionId}");
                return codTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating COD transaction");
                throw;
            }
        }

        public async Task<CodTransaction> Update(CodTransaction codTransaction)
        {
            try
            {
                codTransaction.UpdatedAt = DateTime.Now;
                _context.CodTransactions.Update(codTransaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"COD transaction updated: {codTransaction.CodTransactionId}");
                return codTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating COD transaction: {codTransaction.CodTransactionId}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int codTransactionId)
        {
            try
            {
                var codTransaction = await _context.CodTransactions.FindAsync(codTransactionId);
                if (codTransaction == null)
                    return false;

                _context.CodTransactions.Remove(codTransaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"COD transaction deleted: {codTransactionId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting COD transaction: {codTransactionId}");
                throw;
            }
        }
    }
}
