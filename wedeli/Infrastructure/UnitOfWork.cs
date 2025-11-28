using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;
using wedeli.Repositories.Repo;
using WeDeLi.Repositories.Repo;

namespace wedeli.Infrastructure
{
    /// <summary>
    /// Unit of Work Implementation
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbContextTransaction? _transaction;

        // Repository instances
        private IUserRepository? _users;
        private IRoleRepository? _roles;
        private IOrderRepository? _orders;
        private ICustomerRepository? _customers;
        private IDriverRepository? _drivers;
        private IVehicleRepository? _vehicles;
        private IRouteRepository? _routes;
        private ITripRepository? _trips;
        private IWarehouseStaffRepository? _warehouseStaff;
        private ITransportCompanyRepository? _transportCompanies;
        private ICodTransactionRepository? _codTransactions;
        private IOrderPhotoRepository? _orderPhotos;
        private IOrderStatusHistoryRepository? _orderStatusHistory;
        private ICompanyPartnershipRepository? _companyPartnerships;
        private IOrderTransferRepository? _orderTransfers;
        private IRatingRepository? _ratings;
        private IComplaintRepository? _complaints;
        private ICustomerAddressRepository? _customerAddresses;
        private IPaymentRepository? _payments;
        private IPeriodicInvoiceRepository? _periodicInvoices;

        // Generic repositories cache
        private readonly Dictionary<Type, object> _repositories;

        public UnitOfWork(
            AppDbContext context,
            ILogger<UnitOfWork> logger)
        {
            _context = context;
            _logger = logger;
            _repositories = new Dictionary<Type, object>();
        }

        // ============================================
        // REPOSITORY PROPERTIES (LAZY INITIALIZATION)
        // ============================================

        public IUserRepository Users =>
            _users ??= new UserRepository(_context, _logger);

        public IRoleRepository Roles =>
            _roles ??= new RoleRepository(_context, _logger);

        public IOrderRepository Orders =>
            _orders ??= new OrderRepository(_context, _logger);

        public ICustomerRepository Customers =>
            _customers ??= new CustomerRepository(_context, _logger);

        public IDriverRepository Drivers =>
            _drivers ??= new DriverRepository(_context, _logger);

        public IVehicleRepository Vehicles =>
            _vehicles ??= new VehicleRepository(_context, _logger);

        public IRouteRepository Routes =>
            _routes ??= new RouteRepository(_context, _logger);

        public ITripRepository Trips =>
            _trips ??= new TripRepository(_context, _logger);

        public IWarehouseStaffRepository WarehouseStaff =>
            _warehouseStaff ??= new WarehouseStaffRepository(_context, _logger);

        public ITransportCompanyRepository TransportCompanies =>
            _transportCompanies ??= new TransportCompanyRepository(_context, _logger);

        public ICodTransactionRepository CodTransactions =>
            _codTransactions ??= new CodTransactionRepository(_context, _logger);

        public IOrderPhotoRepository OrderPhotos =>
            _orderPhotos ??= new OrderPhotoRepository(_context, _logger);

        public IOrderStatusHistoryRepository OrderStatusHistory =>
            _orderStatusHistory ??= new OrderStatusHistoryRepository(_context, _logger);

        public ICompanyPartnershipRepository CompanyPartnerships =>
            _companyPartnerships ??= new CompanyPartnershipRepository(_context, _logger);

        public IOrderTransferRepository OrderTransfers =>
            _orderTransfers ??= new OrderTransferRepository(_context, _logger);

        public IRatingRepository Ratings =>
            _ratings ??= new RatingRepository(_context, _logger);

        public IComplaintRepository Complaints =>
            _complaints ??= new ComplaintRepository(_context, _logger);

        public ICustomerAddressRepository CustomerAddresses =>
            _customerAddresses ??= new CustomerAddressRepository(_context, _logger);

        public IPaymentRepository Payments =>
            _payments ??= new PaymentRepository(_context, _logger);

        public IPeriodicInvoiceRepository PeriodicInvoices =>
            _periodicInvoices ??= new PeriodicInvoiceRepository(_context, _logger);

        // ============================================
        // TRANSACTION OPERATIONS
        // ============================================

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                var result = await _context.SaveChangesAsync();
                _logger.LogInformation($"Saved {result} changes to database");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes to database");
                throw;
            }
        }

        public async Task BeginTransactionAsync()
        {
            try
            {
                _transaction = await _context.Database.BeginTransactionAsync();
                _logger.LogInformation("Database transaction started");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting database transaction");
                throw;
            }
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                if (_transaction == null)
                {
                    throw new InvalidOperationException("No active transaction to commit");
                }

                await _transaction.CommitAsync();
                _logger.LogInformation("Database transaction committed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing database transaction");
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_transaction == null)
                {
                    _logger.LogWarning("No active transaction to rollback");
                    return;
                }

                await _transaction.RollbackAsync();
                _logger.LogInformation("Database transaction rolled back");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rolling back database transaction");
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        // ============================================
        // GENERIC REPOSITORY ACCESS
        // ============================================

        public IGenericRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);

            if (_repositories.ContainsKey(type))
            {
                return (IGenericRepository<T>)_repositories[type];
            }

            var repositoryType = typeof(GenericRepository<>).MakeGenericType(type);
            var loggerType = typeof(ILogger<>).MakeGenericType(repositoryType);

            // Get logger from service provider
            var loggerFactory = _context.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger(repositoryType);

            var repository = Activator.CreateInstance(repositoryType, _context, logger);

            if (repository == null)
            {
                throw new InvalidOperationException($"Could not create repository for type {type.Name}");
            }

            _repositories[type] = repository;
            return (IGenericRepository<T>)repository;
        }

        // ============================================
        // DISPOSE
        // ============================================

        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }

            _context.Dispose();
            _logger.LogInformation("UnitOfWork disposed");
        }
    }
}