using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.DTO.Route;
using wedeli.Models.DTO.Order;
using wedeli.Models.DTO.Common;
using wedeli.Models.DTO.Trip;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;

namespace wedeli.Service.Service
{
    /// <summary>
    /// Trip service for managing trip operations
    /// </summary>
    public class TripService : ITripService
    {
        private readonly ITripRepository _tripRepository;
        private readonly ITripOrderRepository _tripOrderRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IRouteRepository _routeRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TripService> _logger;

        public TripService(
            ITripRepository tripRepository,
            ITripOrderRepository tripOrderRepository,
            IOrderRepository orderRepository,
            IRouteRepository routeRepository,
            IVehicleRepository vehicleRepository,
            IDriverRepository driverRepository,
            IMapper mapper,
            ILogger<TripService> logger)
        {
            _tripRepository = tripRepository;
            _tripOrderRepository = tripOrderRepository;
            _orderRepository = orderRepository;
            _routeRepository = routeRepository;
            _vehicleRepository = vehicleRepository;
            _driverRepository = driverRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get trip by ID
        /// </summary>
        public async Task<TripResponseDto> GetTripByIdAsync(int tripId)
        {
            try
            {
                var trip = await _tripRepository.GetByIdAsync(tripId);
                var tripDto = _mapper.Map<TripResponseDto>(trip);

                _logger.LogInformation("Retrieved trip: {TripId}", tripId);
                return tripDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trip: {TripId}", tripId);
                throw;
            }
        }

        /// <summary>
        /// Get trips by route
        /// </summary>
        public async Task<IEnumerable<TripResponseDto>> GetTripsByRouteAsync(int routeId)
        {
            try
            {
                var trips = await _tripRepository.GetByRouteIdAsync(routeId);
                var tripDtos = _mapper.Map<IEnumerable<TripResponseDto>>(trips);

                _logger.LogInformation("Retrieved {Count} trips for route: {RouteId}", trips.Count(), routeId);
                return tripDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trips by route: {RouteId}", routeId);
                throw;
            }
        }

        /// <summary>
        /// Get trips by vehicle
        /// </summary>
        public async Task<IEnumerable<TripResponseDto>> GetTripsByVehicleAsync(int vehicleId)
        {
            try
            {
                var trips = await _tripRepository.GetByVehicleIdAsync(vehicleId);
                var tripDtos = _mapper.Map<IEnumerable<TripResponseDto>>(trips);

                _logger.LogInformation("Retrieved {Count} trips for vehicle: {VehicleId}", trips.Count(), vehicleId);
                return tripDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trips by vehicle: {VehicleId}", vehicleId);
                throw;
            }
        }

        /// <summary>
        /// Get trips by driver
        /// </summary>
        public async Task<IEnumerable<TripResponseDto>> GetTripsByDriverAsync(int driverId)
        {
            try
            {
                var trips = await _tripRepository.GetByDriverIdAsync(driverId);
                var tripDtos = _mapper.Map<IEnumerable<TripResponseDto>>(trips);

                _logger.LogInformation("Retrieved {Count} trips for driver: {DriverId}", trips.Count(), driverId);
                return tripDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trips by driver: {DriverId}", driverId);
                throw;
            }
        }

        /// <summary>
        /// Get trips by date
        /// </summary>
        public async Task<IEnumerable<TripResponseDto>> GetTripsByDateAsync(DateTime date, int? companyId = null)
        {
            try
            {
                var trips = await _tripRepository.GetByDateAsync(date, companyId);
                var tripDtos = _mapper.Map<IEnumerable<TripResponseDto>>(trips);

                _logger.LogInformation("Retrieved {Count} trips for date: {Date}", trips.Count(), date.Date);
                return tripDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trips by date: {Date}", date);
                throw;
            }
        }

        /// <summary>
        /// Get trips by status
        /// </summary>
        public async Task<IEnumerable<TripResponseDto>> GetTripsByStatusAsync(string status, int? companyId = null)
        {
            try
            {
                var trips = await _tripRepository.GetByStatusAsync(status, companyId);
                var tripDtos = _mapper.Map<IEnumerable<TripResponseDto>>(trips);

                _logger.LogInformation("Retrieved {Count} trips with status: {Status}", trips.Count(), status);
                return tripDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trips by status: {Status}", status);
                throw;
            }
        }

        /// <summary>
        /// Get active trips
        /// </summary>
        public async Task<IEnumerable<TripResponseDto>> GetActiveTripsAsync(int? companyId = null)
        {
            try
            {
                var trips = await _tripRepository.GetActiveTripsAsync(companyId);
                var tripDtos = _mapper.Map<IEnumerable<TripResponseDto>>(trips);

                _logger.LogInformation("Retrieved {Count} active trips", trips.Count());
                return tripDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active trips");
                throw;
            }
        }

        /// <summary>
        /// Get return trips
        /// </summary>
        public async Task<IEnumerable<TripResponseDto>> GetReturnTripsAsync(int? companyId = null)
        {
            try
            {
                var trips = await _tripRepository.GetReturnTripsAsync(companyId);
                var tripDtos = _mapper.Map<IEnumerable<TripResponseDto>>(trips);

                _logger.LogInformation("Retrieved {Count} return trips", trips.Count());
                return tripDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving return trips");
                throw;
            }
        }

        /// <summary>
        /// Create new trip
        /// </summary>
        public async Task<TripResponseDto> CreateTripAsync(CreateTripDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                // Verify route exists
                var route = await _routeRepository.GetByIdAsync(dto.RouteId);
                if (route == null)
                    throw new KeyNotFoundException($"Route not found with ID: {dto.RouteId}");

                // Verify vehicle exists
                var vehicle = await _vehicleRepository.GetByIdAsync(dto.VehicleId);
                if (vehicle == null)
                    throw new KeyNotFoundException($"Vehicle not found with ID: {dto.VehicleId}");

                // Verify driver exists
                var driver = await _driverRepository.GetByIdAsync(dto.DriverId);
                if (driver == null)
                    throw new KeyNotFoundException($"Driver not found with ID: {dto.DriverId}");

                var trip = _mapper.Map<Trip>(dto);
                trip.TripStatus = "scheduled";
                trip.TotalOrders = 0;
                trip.TotalWeightKg = 0;

                var createdTrip = await _tripRepository.AddAsync(trip);
                var tripDto = _mapper.Map<TripResponseDto>(createdTrip);

                _logger.LogInformation("Created trip: {TripId} for route: {RouteId}", createdTrip.TripId, dto.RouteId);
                return tripDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trip");
                throw;
            }
        }

        /// <summary>
        /// Update trip
        /// </summary>
        public async Task<TripResponseDto> UpdateTripAsync(int tripId, UpdateTripDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                var trip = await _tripRepository.GetByIdAsync(tripId);
                if (trip == null)
                    throw new KeyNotFoundException($"Trip not found with ID: {tripId}");

                // Verify new vehicle if provided
                if (dto.VehicleId.HasValue && dto.VehicleId > 0)
                {
                    var vehicle = await _vehicleRepository.GetByIdAsync(dto.VehicleId.Value);
                    if (vehicle == null)
                        throw new KeyNotFoundException($"Vehicle not found with ID: {dto.VehicleId}");
                }

                // Verify new driver if provided
                if (dto.DriverId.HasValue && dto.DriverId > 0)
                {
                    var driver = await _driverRepository.GetByIdAsync(dto.DriverId.Value);
                    if (driver == null)
                        throw new KeyNotFoundException($"Driver not found with ID: {dto.DriverId}");
                }

                _mapper.Map(dto, trip);
                var updatedTrip = await _tripRepository.UpdateAsync(trip);
                var tripDto = _mapper.Map<TripResponseDto>(updatedTrip);

                _logger.LogInformation("Updated trip: {TripId}", tripId);
                return tripDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trip: {TripId}", tripId);
                throw;
            }
        }

        /// <summary>
        /// Delete trip
        /// </summary>
        public async Task<bool> DeleteTripAsync(int tripId)
        {
            try
            {
                var result = await _tripRepository.DeleteAsync(tripId);

                _logger.LogInformation("Deleted trip: {TripId}", tripId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting trip: {TripId}", tripId);
                throw;
            }
        }

        /// <summary>
        /// Update trip status
        /// </summary>
        public async Task<bool> UpdateTripStatusAsync(int tripId, string status)
        {
            try
            {
                if (string.IsNullOrEmpty(status))
                    throw new ArgumentException("Status cannot be empty");

                var result = await _tripRepository.UpdateStatusAsync(tripId, status);

                _logger.LogInformation("Updated trip {TripId} status to: {Status}", tripId, status);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trip status: {TripId}", tripId);
                throw;
            }
        }

        /// <summary>
        /// Start trip (change status to in_progress)
        /// </summary>
        public async Task<bool> StartTripAsync(int tripId)
        {
            try
            {
                var trip = await _tripRepository.GetByIdAsync(tripId);
                if (trip == null)
                    throw new KeyNotFoundException($"Trip not found with ID: {tripId}");

                if (trip.TripStatus != "scheduled")
                    throw new InvalidOperationException("Only scheduled trips can be started");

                // Set departure time if not set
                if (!trip.DepartureTime.HasValue)
                    trip.DepartureTime = DateTime.UtcNow;

                var result = await _tripRepository.UpdateStatusAsync(tripId, "in_progress");

                _logger.LogInformation("Started trip: {TripId}", tripId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting trip: {TripId}", tripId);
                throw;
            }
        }

        /// <summary>
        /// Complete trip (change status to completed)
        /// </summary>
        public async Task<bool> CompleteTripAsync(int tripId)
        {
            try
            {
                var trip = await _tripRepository.GetByIdAsync(tripId);
                if (trip == null)
                    throw new KeyNotFoundException($"Trip not found with ID: {tripId}");

                if (trip.TripStatus != "in_progress")
                    throw new InvalidOperationException("Only in_progress trips can be completed");

                // Set arrival time if not set
                if (!trip.ArrivalTime.HasValue)
                    trip.ArrivalTime = DateTime.UtcNow;

                var result = await _tripRepository.UpdateStatusAsync(tripId, "completed");

                _logger.LogInformation("Completed trip: {TripId}", tripId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing trip: {TripId}", tripId);
                throw;
            }
        }

        /// <summary>
        /// Assign order to trip
        /// </summary>
        public async Task<bool> AssignOrderToTripAsync(int tripId, int orderId, int? sequenceNumber = null)
        {
            try
            {
                var trip = await _tripRepository.GetByIdAsync(tripId);
                if (trip == null)
                    throw new KeyNotFoundException($"Trip not found with ID: {tripId}");

                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                    throw new KeyNotFoundException($"Order not found with ID: {orderId}");

                // Assign order to trip
                var result = await _tripOrderRepository.AssignOrderToTripAsync(tripId, orderId, sequenceNumber);

                // Update trip statistics
                var tripOrders = await _tripOrderRepository.GetByTripIdAsync(tripId);
                var tripOrdersList = tripOrders.ToList();
                var totalOrders = tripOrdersList.Count;
                
                // Calculate total weight (this assumes orders have WeightKg or similar)
                var totalWeight = tripOrdersList.Sum(to => to.Order?.WeightKg ?? 0);

                await _tripRepository.UpdateTripStatisticsAsync(tripId, totalOrders, totalWeight);

                _logger.LogInformation("Assigned order {OrderId} to trip {TripId}", orderId, tripId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning order to trip: {OrderId}, {TripId}", orderId, tripId);
                throw;
            }
        }

        /// <summary>
        /// Remove order from trip
        /// </summary>
        public async Task<bool> RemoveOrderFromTripAsync(int tripId, int orderId)
        {
            try
            {
                var trip = await _tripRepository.GetByIdAsync(tripId);
                if (trip == null)
                    throw new KeyNotFoundException($"Trip not found with ID: {tripId}");

                var tripOrder = await _tripOrderRepository.GetByOrderIdAsync(orderId);
                if (tripOrder == null || tripOrder.TripId != tripId)
                    throw new KeyNotFoundException($"Order {orderId} is not assigned to trip {tripId}");

                var result = await _tripOrderRepository.RemoveOrderFromTripAsync(tripOrder.TripOrderId);

                // Update trip statistics
                var tripOrders = await _tripOrderRepository.GetByTripIdAsync(tripId);
                var tripOrdersList = tripOrders.ToList();
                var totalOrders = tripOrdersList.Count;
                var totalWeight = tripOrdersList.Sum(to => to.Order?.WeightKg ?? 0);

                await _tripRepository.UpdateTripStatisticsAsync(tripId, totalOrders, totalWeight);

                _logger.LogInformation("Removed order {OrderId} from trip {TripId}", orderId, tripId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing order from trip: {OrderId}, {TripId}", orderId, tripId);
                throw;
            }
        }

        /// <summary>
        /// Get trip orders
        /// </summary>
        public async Task<IEnumerable<TripOrderDto>> GetTripOrdersAsync(int tripId)
        {
            try
            {
                var trip = await _tripRepository.GetByIdAsync(tripId);
                if (trip == null)
                    throw new KeyNotFoundException($"Trip not found with ID: {tripId}");

                var tripOrders = await _tripOrderRepository.GetByTripIdAsync(tripId);
                
                var tripOrderDtos = tripOrders.Select(to => new TripOrderDto
                {
                    OrderId = to.OrderId,
                    TrackingCode = to.Order?.TrackingCode ?? "",
                    ReceiverName = to.Order?.ReceiverName ?? "",
                    ReceiverAddress = to.Order?.ReceiverAddress ?? "",
                    WeightKg = to.Order?.WeightKg ?? 0,
                    OrderStatus = to.Order?.OrderStatus ?? "",
                    SequenceNumber = to.SequenceNumber,
                    PickupConfirmed = to.PickupConfirmed ?? false,
                    DeliveryConfirmed = to.DeliveryConfirmed ?? false
                }).ToList();

                _logger.LogInformation("Retrieved {Count} orders for trip: {TripId}", tripOrderDtos.Count, tripId);
                return tripOrderDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trip orders: {TripId}", tripId);
                throw;
            }
        }
    }
}
