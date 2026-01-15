using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.Route;
using wedeli.Models.DTO.Order;
using wedeli.Models.DTO.Common;
using wedeli.Models.DTO.Trip;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Trip service interface for trip management
    /// </summary>
    public interface ITripService
    {
        Task<TripResponseDto> GetTripByIdAsync(int tripId);
        Task<IEnumerable<TripResponseDto>> GetTripsByRouteAsync(int routeId);
        Task<IEnumerable<TripResponseDto>> GetTripsByVehicleAsync(int vehicleId);
        Task<IEnumerable<TripResponseDto>> GetTripsByDriverAsync(int driverId);
        Task<IEnumerable<TripResponseDto>> GetTripsByDateAsync(DateTime date, int? companyId = null);
        Task<IEnumerable<TripResponseDto>> GetTripsByStatusAsync(string status, int? companyId = null);
        Task<IEnumerable<TripResponseDto>> GetActiveTripsAsync(int? companyId = null);
        Task<IEnumerable<TripResponseDto>> GetReturnTripsAsync(int? companyId = null);
        
        Task<TripResponseDto> CreateTripAsync(CreateTripDto dto);
        Task<TripResponseDto> UpdateTripAsync(int tripId, UpdateTripDto dto);
        Task<bool> DeleteTripAsync(int tripId);
        Task<bool> UpdateTripStatusAsync(int tripId, string status);
        Task<bool> StartTripAsync(int tripId);
        Task<bool> CompleteTripAsync(int tripId);
        
        // Trip-Order management
        Task<bool> AssignOrderToTripAsync(int tripId, int orderId, int? sequenceNumber = null);
        Task<bool> RemoveOrderFromTripAsync(int tripId, int orderId);
        Task<IEnumerable<TripOrderDto>> GetTripOrdersAsync(int tripId);
    }
}
