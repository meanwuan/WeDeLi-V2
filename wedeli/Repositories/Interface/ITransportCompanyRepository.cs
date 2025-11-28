using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface ITransportCompanyRepository : IGenericRepository<TransportCompany>
    {
        /// <summary>
        /// Get company by name
        /// </summary>
        Task<TransportCompany?> GetCompanyByNameAsync(string companyName);

        /// <summary>
        /// Get company with all related data
        /// </summary>
        Task<TransportCompany?> GetCompanyWithDetailsAsync(int companyId);

        /// <summary>
        /// Get all active companies
        /// </summary>
        Task<List<TransportCompany>> GetActiveCompaniesAsync();

        /// <summary>
        /// Get companies by province/city
        /// </summary>
        Task<List<TransportCompany>> GetCompaniesByLocationAsync(string province);

        /// <summary>
        /// Search companies by name or code
        /// </summary>
        Task<List<TransportCompany>> SearchCompaniesAsync(string keyword);

        /// <summary>
        /// Get company statistics
        /// </summary>
        Task<CompanyStatistics> GetCompanyStatisticsAsync(int companyId);

        /// <summary>
        /// Get company vehicles count
        /// </summary>
        Task<int> GetVehiclesCountAsync(int companyId);

        /// <summary>
        /// Get company drivers count
        /// </summary>
        Task<int> GetDriversCountAsync(int companyId);

        /// <summary>
        /// Get company orders count
        /// </summary>
        Task<int> GetOrdersCountAsync(int companyId);

        /// <summary>
        /// Check if company code exists
        /// </summary>
        // Task<bool> CompanyCodeExistsAsync(string companyCode);
    }

    public class CompanyStatistics
    {
        public int TotalVehicles { get; set; }
        public int ActiveVehicles { get; set; }
        public int TotalDrivers { get; set; }
        public int ActiveDrivers { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalRoutes { get; set; }
        public int TotalTrips { get; set; }
    }
}