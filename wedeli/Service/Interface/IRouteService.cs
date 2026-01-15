using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.Route;
using wedeli.Models.DTO.Common;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Route service interface
    /// </summary>
    public interface IRouteService
    {
        Task<IEnumerable<RouteResponseDto>> GetAllRoutesAsync();
        Task<RouteResponseDto> GetRouteByIdAsync(int routeId);
        Task<IEnumerable<RouteResponseDto>> GetRoutesByCompanyAsync(int companyId);
        Task<IEnumerable<RouteResponseDto>> GetActiveRoutesAsync(int companyId);
        Task<IEnumerable<RouteResponseDto>> SearchRoutesAsync(string originProvince, string destinationProvince);
        Task<RouteResponseDto> GetOptimalRouteAsync(string originProvince, string destProvince, int? companyId = null);
        Task<ShippingFeeResponseDto> CalculateShippingFeeAsync(CalculateShippingFeeDto dto);
        Task<IEnumerable<CompanyRouteRecommendationDto>> GetCompaniesForRouteAsync(string originProvince, string destProvince);
        Task<RouteResponseDto> CreateRouteAsync(CreateRouteDto dto);
        Task<RouteResponseDto> UpdateRouteAsync(int routeId, UpdateRouteDto dto);
        Task<bool> DeleteRouteAsync(int routeId);
        Task<bool> ToggleRouteStatusAsync(int routeId, bool isActive);
    }
}
