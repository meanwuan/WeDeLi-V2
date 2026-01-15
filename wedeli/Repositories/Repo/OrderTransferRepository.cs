using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Repo
{
    /// <summary>
    /// Order transfer repository for managing order transfer data access
    /// </summary>
    public class OrderTransferRepository : IOrderTransferRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderTransferRepository> _logger;

        public OrderTransferRepository(AppDbContext context, ILogger<OrderTransferRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // IBaseRepository Implementation

        /// <summary>
        /// Get transfer by ID
        /// </summary>
        public async Task<OrderTransfer> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Transfer ID must be greater than 0");

                var transfer = await _context.OrderTransfers
                    .Include(ot => ot.Order)
                    // NOTE: FromCompany/ToCompany (TransportCompany) is in Platform DB, cannot be included
                    .Include(ot => ot.OriginalVehicle)
                    .Include(ot => ot.NewVehicle)
                    .FirstOrDefaultAsync(ot => ot.TransferId == id);

                if (transfer == null)
                    throw new KeyNotFoundException($"Transfer not found with ID: {id}");

                _logger.LogInformation("Retrieved transfer: {TransferId}", id);
                return transfer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transfer: {TransferId}", id);
                throw;
            }
        }

        /// <summary>
        /// Get all transfers
        /// </summary>
        public async Task<IEnumerable<OrderTransfer>> GetAllAsync()
        {
            try
            {
                var transfers = await _context.OrderTransfers
                    .Include(ot => ot.Order)
                    // NOTE: FromCompany/ToCompany (TransportCompany) is in Platform DB, cannot be included
                    .OrderByDescending(ot => ot.TransferredAt)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} transfers", transfers.Count);
                return transfers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all transfers");
                throw;
            }
        }

        /// <summary>
        /// Add new transfer
        /// </summary>
        public async Task<OrderTransfer> AddAsync(OrderTransfer entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                entity.TransferredAt = DateTime.UtcNow;
                _context.OrderTransfers.Add(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created transfer: {TransferId}", entity.TransferId);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transfer");
                throw;
            }
        }

        /// <summary>
        /// Update transfer
        /// </summary>
        public async Task<OrderTransfer> UpdateAsync(OrderTransfer entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                if (entity.TransferId <= 0)
                    throw new ArgumentException("Transfer ID must be greater than 0");

                var existingTransfer = await _context.OrderTransfers
                    .FirstOrDefaultAsync(ot => ot.TransferId == entity.TransferId);

                if (existingTransfer == null)
                    throw new KeyNotFoundException($"Transfer not found with ID: {entity.TransferId}");

                _context.Entry(existingTransfer).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated transfer: {TransferId}", entity.TransferId);
                return existingTransfer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transfer: {TransferId}", entity?.TransferId);
                throw;
            }
        }

        /// <summary>
        /// Delete transfer
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Transfer ID must be greater than 0");

                var transfer = await _context.OrderTransfers.FirstOrDefaultAsync(ot => ot.TransferId == id);
                if (transfer == null)
                    throw new KeyNotFoundException($"Transfer not found with ID: {id}");

                _context.OrderTransfers.Remove(transfer);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted transfer: {TransferId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transfer: {TransferId}", id);
                throw;
            }
        }

        /// <summary>
        /// Check if transfer exists
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.OrderTransfers.AnyAsync(ot => ot.TransferId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking transfer existence: {TransferId}", id);
                throw;
            }
        }

        /// <summary>
        /// Count all transfers
        /// </summary>
        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.OrderTransfers.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting transfers");
                throw;
            }
        }

        // IOrderTransferRepository Implementation

        /// <summary>
        /// Get transfers by order
        /// </summary>
        public async Task<IEnumerable<OrderTransfer>> GetByOrderIdAsync(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("Order ID must be greater than 0");

                var transfers = await _context.OrderTransfers
                    .Where(ot => ot.OrderId == orderId)
                    .Include(ot => ot.Order)
                    // NOTE: FromCompany/ToCompany (TransportCompany) is in Platform DB, cannot be included
                    .OrderByDescending(ot => ot.TransferredAt)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} transfers for order: {OrderId}", transfers.Count, orderId);
                return transfers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transfers by order: {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Get transfers from company (outgoing)
        /// </summary>
        public async Task<IEnumerable<OrderTransfer>> GetByFromCompanyAsync(int companyId)
        {
            try
            {
                if (companyId <= 0)
                    throw new ArgumentException("Company ID must be greater than 0");

                var transfers = await _context.OrderTransfers
                    .Where(ot => ot.FromCompanyId == companyId)
                    .Include(ot => ot.Order)
                    // NOTE: FromCompany/ToCompany (TransportCompany) is in Platform DB, cannot be included
                    .OrderByDescending(ot => ot.TransferredAt)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} outgoing transfers for company: {CompanyId}", transfers.Count, companyId);
                return transfers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving outgoing transfers: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Get transfers to company (incoming)
        /// </summary>
        public async Task<IEnumerable<OrderTransfer>> GetByToCompanyAsync(int companyId)
        {
            try
            {
                if (companyId <= 0)
                    throw new ArgumentException("Company ID must be greater than 0");

                var transfers = await _context.OrderTransfers
                    .Where(ot => ot.ToCompanyId == companyId)
                    .Include(ot => ot.Order)
                    // NOTE: FromCompany/ToCompany (TransportCompany) is in Platform DB, cannot be included
                    .OrderByDescending(ot => ot.TransferredAt)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} incoming transfers for company: {CompanyId}", transfers.Count, companyId);
                return transfers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving incoming transfers: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Get transfers by status
        /// </summary>
        public async Task<IEnumerable<OrderTransfer>> GetByStatusAsync(string status, int? companyId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(status))
                    throw new ArgumentException("Status cannot be empty");

                var query = _context.OrderTransfers
                    .Where(ot => ot.TransferStatus == status)
                    // NOTE: FromCompany/ToCompany (TransportCompany) is in Platform DB, cannot be included
                    .Include(ot => ot.Order)
                    .AsQueryable();

                if (companyId.HasValue && companyId > 0)
                {
                    query = query.Where(ot => ot.FromCompanyId == companyId || ot.ToCompanyId == companyId);
                }

                var transfers = await query
                    .OrderByDescending(ot => ot.TransferredAt)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} transfers with status: {Status}", transfers.Count, status);
                return transfers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transfers by status: {Status}", status);
                throw;
            }
        }

        /// <summary>
        /// Update transfer status
        /// </summary>
        public async Task<bool> UpdateStatusAsync(int transferId, string status)
        {
            try
            {
                if (transferId <= 0)
                    throw new ArgumentException("Transfer ID must be greater than 0");

                if (string.IsNullOrEmpty(status))
                    throw new ArgumentException("Status cannot be empty");

                var transfer = await _context.OrderTransfers.FirstOrDefaultAsync(ot => ot.TransferId == transferId);
                if (transfer == null)
                    throw new KeyNotFoundException($"Transfer not found with ID: {transferId}");

                transfer.TransferStatus = status;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated transfer {TransferId} status to: {Status}", transferId, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transfer status: {TransferId}", transferId);
                throw;
            }
        }

        /// <summary>
        /// Accept transfer with optional new vehicle
        /// </summary>
        public async Task<bool> AcceptTransferAsync(int transferId, int? newVehicleId = null)
        {
            try
            {
                if (transferId <= 0)
                    throw new ArgumentException("Transfer ID must be greater than 0");

                var transfer = await _context.OrderTransfers.FirstOrDefaultAsync(ot => ot.TransferId == transferId);
                if (transfer == null)
                    throw new KeyNotFoundException($"Transfer not found with ID: {transferId}");

                transfer.TransferStatus = "accepted";
                transfer.AcceptedAt = DateTime.UtcNow;

                if (newVehicleId.HasValue && newVehicleId > 0)
                {
                    transfer.NewVehicleId = newVehicleId;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Accepted transfer: {TransferId}", transferId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting transfer: {TransferId}", transferId);
                throw;
            }
        }

        /// <summary>
        /// Create transfer record
        /// </summary>
        public async Task<OrderTransfer> CreateTransferAsync(int orderId, int fromCompanyId, int toCompanyId, string reason, int transferredBy)
        {
            try
            {
                if (orderId <= 0 || fromCompanyId <= 0 || toCompanyId <= 0)
                    throw new ArgumentException("IDs must be greater than 0");

                if (string.IsNullOrEmpty(reason))
                    throw new ArgumentException("Reason cannot be empty");

                var transfer = new OrderTransfer
                {
                    OrderId = orderId,
                    FromCompanyId = fromCompanyId,
                    ToCompanyId = toCompanyId,
                    TransferReason = reason,
                    TransferredBy = transferredBy,
                    TransferStatus = "pending",
                    TransferredAt = DateTime.UtcNow
                };

                _context.OrderTransfers.Add(transfer);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created transfer: {TransferId} for order: {OrderId}", transfer.TransferId, orderId);
                return transfer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transfer");
                throw;
            }
        }
    }
}
