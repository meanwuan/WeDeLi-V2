using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Interface
{
    public interface IRouteRepository : IBaseRepository<wedeli.Models.Domain.Route>
    {
        Task<IEnumerable<wedeli.Models.Domain.Route>> GetAllWithCompanyAsync();
        Task<IEnumerable<wedeli.Models.Domain.Route>> GetByCompanyIdAsync(int companyId);
        Task<IEnumerable<wedeli.Models.Domain.Route>> GetActiveRoutesAsync(int companyId);
        Task<IEnumerable<wedeli.Models.Domain.Route>> SearchRoutesAsync(string originProvince, string destinationProvince);
        Task<wedeli.Models.Domain.Route> GetOptimalRouteAsync(string originProvince, string destProvince, int? companyId = null);
        Task<bool> ToggleActiveStatusAsync(int routeId, bool isActive);
    }
}
