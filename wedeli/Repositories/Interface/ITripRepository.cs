using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface ITripRepository : IBaseRepository<Trip>
    {
        Task<IEnumerable<Trip>> GetByRouteIdAsync(int routeId);
        Task<IEnumerable<Trip>> GetByVehicleIdAsync(int vehicleId);
        Task<IEnumerable<Trip>> GetByDriverIdAsync(int driverId);
        Task<IEnumerable<Trip>> GetByDateAsync(DateTime date, int? companyId = null);
        Task<IEnumerable<Trip>> GetByStatusAsync(string status, int? companyId = null);
        Task<bool> UpdateStatusAsync(int tripId, string newStatus);
        Task<bool> UpdateTripTimesAsync(int tripId, DateTime? departureTime = null, DateTime? arrivalTime = null);
        Task<bool> UpdateTripStatisticsAsync(int tripId, int totalOrders, decimal totalWeightKg);
        Task<IEnumerable<Trip>> GetActiveTripsAsync(int? companyId = null);
        Task<IEnumerable<Trip>> GetReturnTripsAsync(int? companyId = null);
    }
}
