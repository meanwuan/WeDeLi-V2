using System;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    /// <summary>
    /// Unit of Work pattern interface for managing all repository operations
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // User & Auth
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        
        // Customer
        ICustomerRepository Customers { get; }
        ICustomerAddressRepository CustomerAddresses { get; }
        
        // Company & Fleet
        ITransportCompanyRepository TransportCompanies { get; }
        IRouteRepository Routes { get; }
        IVehicleRepository Vehicles { get; }
        IDriverRepository Drivers { get; }
        IWarehouseStaffRepository WarehouseStaff { get; }
        
        // Orders
        IOrderRepository Orders { get; }
        IOrderStatusHistoryRepository OrderStatusHistory { get; }
        IOrderPhotoRepository OrderPhotos { get; }
        
        // Trips
        ITripRepository Trips { get; }
        ITripOrderRepository TripOrders { get; }
        
        // Payments
        IPaymentRepository Payments { get; }
        ICODTransactionRepository CODTransactions { get; }
        IDriverCodSummaryRepository DriverCODSummaries { get; }
        IPeriodicInvoiceRepository PeriodicInvoices { get; }
        
        // Complaints & Ratings
        IComplaintRepository Complaints { get; }
        IRatingRepository Ratings { get; }
        
        // Notifications
        INotificationRepository Notifications { get; }
        
        // Partnerships
        ICompanyPartnershipRepository CompanyPartnerships { get; }
        IOrderTransferRepository OrderTransfers { get; }
        
        // Save changes
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
