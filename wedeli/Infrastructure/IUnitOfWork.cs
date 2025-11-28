using wedeli.Repositories.Interface;

namespace wedeli.Infrastructure
{
    /// <summary>
    /// Unit of Work Pattern - Manages transactions and coordinates Repository operations
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // ============================================
        // REPOSITORY PROPERTIES
        // ============================================

        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IOrderRepository Orders { get; }
        ICustomerRepository Customers { get; }
        IDriverRepository Drivers { get; }
        IVehicleRepository Vehicles { get; }
        IRouteRepository Routes { get; }
        ITripRepository Trips { get; }
        IWarehouseStaffRepository WarehouseStaff { get; }
        ITransportCompanyRepository TransportCompanies { get; }
        ICodTransactionRepository CodTransactions { get; }
        IOrderPhotoRepository OrderPhotos { get; }
        IOrderStatusHistoryRepository OrderStatusHistory { get; }
        ICompanyPartnershipRepository CompanyPartnerships { get; }
        IOrderTransferRepository OrderTransfers { get; }
        IRatingRepository Ratings { get; }
        IComplaintRepository Complaints { get; }
        ICustomerAddressRepository CustomerAddresses { get; }
        IPaymentRepository Payments { get; }
        IPeriodicInvoiceRepository PeriodicInvoices { get; }

        // ============================================
        // TRANSACTION OPERATIONS
        // ============================================

        /// <summary>
        /// Save all changes to database
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Begin database transaction
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Commit current transaction
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rollback current transaction
        /// </summary>
        Task RollbackTransactionAsync();

        // ============================================
        // GENERIC REPOSITORY ACCESS
        // ============================================

        /// <summary>
        /// Get generic repository for any entity type
        /// </summary>
        IGenericRepository<T> Repository<T>() where T : class;
    }
}