using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;

namespace WeDeLi.Repositories.Repo
{
    public class OrderTransferRepository : IOrderTransferRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderTransferRepository> _logger;

        public OrderTransferRepository(AppDbContext context, ILogger<OrderTransferRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<OrderTransfer?> GetTransferByIdAsync(int transferId)
        {
            try
            {
                return await _context.OrderTransfers
                    .Include(t => t.Order)
                    .Include(t => t.FromCompany)
                    .Include(t => t.ToCompany)
                    .Include(t => t.TransferredByNavigation)
                    .Include(t => t.OriginalVehicle)
                    .Include(t => t.NewVehicle)
                    .FirstOrDefaultAsync(t => t.TransferId == transferId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting transfer by ID: {transferId}");
                throw;
            }
        }

        public async Task<List<OrderTransfer>> GetOrderTransfersAsync(int orderId)
        {
            try
            {
                return await _context.OrderTransfers
                    .Include(t => t.FromCompany)
                    .Include(t => t.ToCompany)
                    .Include(t => t.TransferredByNavigation)
                    .Include(t => t.OriginalVehicle)
                    .Include(t => t.NewVehicle)
                    .Where(t => t.OrderId == orderId)
                    .OrderByDescending(t => t.TransferredAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting transfers for order: {orderId}");
                throw;
            }
        }

        public async Task<List<OrderTransfer>> GetCompanyTransfersAsync(int companyId, string? status = null)
        {
            try
            {
                var query = _context.OrderTransfers
                    .Include(t => t.Order)
                    .Include(t => t.FromCompany)
                    .Include(t => t.ToCompany)
                    .Include(t => t.TransferredByNavigation)
                    .Where(t => t.FromCompanyId == companyId || t.ToCompanyId == companyId);

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(t => t.TransferStatus == status);
                }

                return await query
                    .OrderByDescending(t => t.TransferredAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting transfers for company: {companyId}");
                throw;
            }
        }

        public async Task<OrderTransfer> CreateTransferAsync(OrderTransfer transfer)
        {
            try
            {
                transfer.TransferredAt = DateTime.Now;
                transfer.TransferStatus = "pending";

                await _context.OrderTransfers.AddAsync(transfer);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Transfer created: Order {transfer.OrderId} from Company {transfer.FromCompanyId} to {transfer.ToCompanyId}");

                return transfer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transfer");
                throw;
            }
        }

        public async Task<OrderTransfer> UpdateTransferAsync(OrderTransfer transfer)
        {
            try
            {
                _context.OrderTransfers.Update(transfer);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Transfer updated: {transfer.TransferId}");

                return transfer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating transfer: {transfer.TransferId}");
                throw;
            }
        }

        public async Task<bool> AcceptTransferAsync(int transferId, int acceptedBy, int? newVehicleId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transfer = await _context.OrderTransfers
                    .Include(t => t.Order)
                    .FirstOrDefaultAsync(t => t.TransferId == transferId);

                if (transfer == null)
                {
                    _logger.LogWarning($"Transfer not found: {transferId}");
                    return false;
                }

                if (transfer.TransferStatus != "pending")
                {
                    _logger.LogWarning($"Transfer {transferId} is not pending. Current status: {transfer.TransferStatus}");
                    return false;
                }

                // Update transfer status
                transfer.TransferStatus = "accepted";
                transfer.AcceptedAt = DateTime.Now;
                transfer.NewVehicleId = newVehicleId;

                // Update order vehicle assignment
                if (transfer.Order != null && newVehicleId.HasValue)
                {
                    transfer.Order.VehicleId = newVehicleId.Value;
                    _context.Orders.Update(transfer.Order);
                }

                // Update partnership statistics
                var partnership = await _context.CompanyPartnerships
                    .FirstOrDefaultAsync(p =>
                        p.CompanyId == transfer.FromCompanyId &&
                        p.PartnerCompanyId == transfer.ToCompanyId &&
                        p.IsActive == true);

                if (partnership != null)
                {
                    partnership.TotalTransferredOrders = (partnership.TotalTransferredOrders ?? 0) + 1;
                    partnership.UpdatedAt = DateTime.Now;
                    _context.CompanyPartnerships.Update(partnership);
                }

                _context.OrderTransfers.Update(transfer);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Transfer {transferId} accepted by user {acceptedBy} with vehicle {newVehicleId}");

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error accepting transfer: {transferId}");
                throw;
            }
        }

        public async Task<bool> RejectTransferAsync(int transferId, string reason)
        {
            try
            {
                var transfer = await _context.OrderTransfers
                    .FirstOrDefaultAsync(t => t.TransferId == transferId);

                if (transfer == null)
                {
                    _logger.LogWarning($"Transfer not found: {transferId}");
                    return false;
                }

                if (transfer.TransferStatus != "pending")
                {
                    _logger.LogWarning($"Transfer {transferId} is not pending. Current status: {transfer.TransferStatus}");
                    return false;
                }

                transfer.TransferStatus = "rejected";
                transfer.AdminNotes = reason;

                _context.OrderTransfers.Update(transfer);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Transfer {transferId} rejected. Reason: {reason}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rejecting transfer: {transferId}");
                throw;
            }
        }

        public async Task<List<OrderTransfer>> GetPendingTransfersForCompanyAsync(int companyId)
        {
            try
            {
                return await _context.OrderTransfers
                    .Include(t => t.Order)
                    .Include(t => t.FromCompany)
                    .Include(t => t.ToCompany)
                    .Include(t => t.TransferredByNavigation)
                    .Include(t => t.OriginalVehicle)
                    .Where(t => t.ToCompanyId == companyId && t.TransferStatus == "pending")
                    .OrderBy(t => t.TransferredAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting pending transfers for company: {companyId}");
                throw;
            }
        }
    }
}