using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using wedeli.Infrastructure;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Repo
{
    /// <summary>
    /// COD Transaction repository for COD data access operations
    /// </summary>
    public class CodTransactionRepository : ICODTransactionRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CodTransactionRepository> _logger;

        public CodTransactionRepository(AppDbContext context, ILogger<CodTransactionRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get COD transaction by ID
        /// </summary>
        public async Task<CodTransaction> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("COD transaction ID must be greater than 0", nameof(id));

                var transaction = await _context.CodTransactions
                    .Include(c => c.Order)
                    .Include(c => c.CollectedByDriver)
                    .FirstOrDefaultAsync(c => c.CodTransactionId == id);

                if (transaction == null)
                    throw new KeyNotFoundException($"COD transaction {id} not found");

                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving COD transaction: {CodId}", id);
                throw;
            }
        }

        /// <summary>
        /// Get COD by order ID
        /// </summary>
        public async Task<CodTransaction> GetByOrderIdAsync(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("Order ID must be greater than 0", nameof(orderId));

                var transaction = await _context.CodTransactions
                    .Include(c => c.Order)
                    .Include(c => c.CollectedByDriver)
                    .FirstOrDefaultAsync(c => c.OrderId == orderId);

                if (transaction == null)
                    throw new KeyNotFoundException($"COD transaction for order ID {orderId} not found.");
                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving COD by order ID: {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Get COD transactions by driver
        /// </summary>
        public async Task<IEnumerable<CodTransaction>> GetByDriverIdAsync(int driverId, string? status = null)
        {
            try
            {
                if (driverId <= 0)
                    throw new ArgumentException("Driver ID must be greater than 0", nameof(driverId));

                var query = _context.CodTransactions
                    .Include(c => c.Order)
                    .Include(c => c.CollectedByDriverNavigation)
                    .Where(c => c.CollectedByDriver == driverId);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(c => c.CollectionStatus == status);

                var transactions = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return transactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving COD transactions for driver: {DriverId}", driverId);
                throw;
            }
        }

        /// <summary>
        /// Get pending COD collections
        /// </summary>
        public async Task<IEnumerable<CodTransaction>> GetPendingCollectionsAsync(int driverId)
        {
            try
            {
                if (driverId <= 0)
                    throw new ArgumentException("Driver ID must be greater than 0", nameof(driverId));

                var transactions = await _context.CodTransactions
                    .Include(c => c.Order)
                    .Include(c => c.CollectedByDriverNavigation)
                    .Where(c => c.CollectedByDriver == driverId && c.CollectionStatus == "pending")
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return transactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending COD collections for driver: {DriverId}", driverId);
                throw;
            }
        }

        /// <summary>
        /// Get pending COD submissions
        /// </summary>
        public async Task<IEnumerable<CodTransaction>> GetPendingSubmissionsAsync(int? driverId = null)
        {
            try
            {
                var query = _context.CodTransactions
                    .Include(c => c.Order)
                    .Include(c => c.CollectedByDriverNavigation)
                    .Where(c => c.CollectionStatus == "collected");

                if (driverId.HasValue && driverId > 0)
                    query = query.Where(c => c.CollectedByDriver == driverId);

                var transactions = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return transactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending COD submissions");
                throw;
            }
        }

        /// <summary>
        /// Get driver pending amount
        /// </summary>
        public async Task<decimal> GetDriverPendingAmountAsync(int driverId)
        {
            try
            {
                if (driverId <= 0)
                    throw new ArgumentException("Driver ID must be greater than 0", nameof(driverId));

                var totalAmount = await _context.CodTransactions
                    .Where(c => c.CollectedByDriver == driverId && c.CollectionStatus == "collected")
                    .SumAsync(c => c.CodAmount);

                return totalAmount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending amount for driver: {DriverId}", driverId);
                throw;
            }
        }

        /// <summary>
        /// Update COD status
        /// </summary>
        public async Task<bool> UpdateStatusAsync(int codId, string newStatus)
        {
            try
            {
                if (codId <= 0)
                    throw new ArgumentException("COD transaction ID must be greater than 0", nameof(codId));

                if (string.IsNullOrEmpty(newStatus))
                    throw new ArgumentNullException(nameof(newStatus));

                var transaction = await _context.CodTransactions.FirstOrDefaultAsync(c => c.CodTransactionId == codId);

                if (transaction == null)
                    throw new KeyNotFoundException($"COD transaction {codId} not found");

                transaction.CollectionStatus = newStatus;
                transaction.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated COD status - CodId: {CodId}, NewStatus: {NewStatus}", codId, newStatus);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating COD status: {CodId}", codId);
                throw;
            }
        }

        /// <summary>
        /// Get all COD transactions
        /// </summary>
        public async Task<IEnumerable<CodTransaction>> GetAllAsync()
        {
            try
            {
                var transactions = await _context.CodTransactions
                    .Include(c => c.Order)
                    .Include(c => c.CollectedByDriver)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return transactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all COD transactions");
                throw;
            }
        }

        /// <summary>
        /// Add new COD transaction
        /// </summary>
        public async Task<CodTransaction> AddAsync(CodTransaction entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                // Check if order already has COD
                if (await _context.CodTransactions.AnyAsync(c => c.OrderId == entity.OrderId))
                    throw new InvalidOperationException($"COD transaction for order {entity.OrderId} already exists");

                entity.CreatedAt = DateTime.UtcNow;
                entity.CollectionStatus = "pending";

                _context.CodTransactions.Add(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Added new COD transaction: {CodId}", entity.CodTransactionId);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding COD transaction");
                throw;
            }
        }

        /// <summary>
        /// Update COD transaction
        /// </summary>
        public async Task<CodTransaction> UpdateAsync(CodTransaction entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var existingTransaction = await _context.CodTransactions
                    .FirstOrDefaultAsync(c => c.CodTransactionId == entity.CodTransactionId);

                if (existingTransaction == null)
                    throw new KeyNotFoundException($"COD transaction {entity.CodTransactionId} not found");

                existingTransaction.CodAmount = entity.CodAmount;
                existingTransaction.CollectionStatus = entity.CollectionStatus;
                existingTransaction.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated COD transaction: {CodId}", entity.CodTransactionId);
                return existingTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating COD transaction: {CodId}", entity?.CodTransactionId);
                throw;
            }
        }

        /// <summary>
        /// Delete COD transaction
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("COD transaction ID must be greater than 0", nameof(id));

                var transaction = await _context.CodTransactions.FirstOrDefaultAsync(c => c.CodTransactionId == id);

                if (transaction == null)
                    throw new KeyNotFoundException($"COD transaction {id} not found");

                _context.CodTransactions.Remove(transaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted COD transaction: {CodId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting COD transaction: {CodId}", id);
                throw;
            }
        }

        /// <summary>
        /// Check if COD transaction exists
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.CodTransactions.AnyAsync(c => c.CodTransactionId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if COD transaction exists: {CodId}", id);
                throw;
            }
        }

        /// <summary>
        /// Count total COD transactions
        /// </summary>
        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.CodTransactions.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting COD transactions");
                throw;
            }
        }
    }
}
