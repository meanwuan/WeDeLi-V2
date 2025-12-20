using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using wedeli.Models.DTO.Vehicle;
using wedeli.Service.Interface;

namespace wedeli.Hubs;

/// <summary>
/// SignalR Hub for realtime vehicle location tracking
/// </summary>
[Authorize]
public class VehicleTrackingHub : Hub
{
    private readonly IVehicleLocationService _locationService;
    private readonly ILogger<VehicleTrackingHub> _logger;

    public VehicleTrackingHub(
        IVehicleLocationService locationService,
        ILogger<VehicleTrackingHub> logger)
    {
        _locationService = locationService;
        _logger = logger;
    }

    /// <summary>
    /// Admin joins company group to receive vehicle updates
    /// </summary>
    public async Task JoinCompanyGroup(int companyId)
    {
        var groupName = GetCompanyGroupName(companyId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation(
            "Client {ConnectionId} joined company group {CompanyId}", 
            Context.ConnectionId, 
            companyId);

        // Send current vehicle locations immediately
        var locations = await _locationService.GetCompanyVehicleLocationsAsync(companyId);
        await Clients.Caller.SendAsync("ReceiveCompanyVehicles", locations);
    }

    /// <summary>
    /// Leave company group
    /// </summary>
    public async Task LeaveCompanyGroup(int companyId)
    {
        var groupName = GetCompanyGroupName(companyId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation(
            "Client {ConnectionId} left company group {CompanyId}", 
            Context.ConnectionId, 
            companyId);
    }

    /// <summary>
    /// Driver sends location update
    /// </summary>
    public async Task UpdateLocation(UpdateVehicleLocationDto locationDto)
    {
        try
        {
            var updatedLocation = await _locationService.UpdateLocationAsync(locationDto);
            
            // Broadcast to company admin group
            var groupName = GetCompanyGroupName(updatedLocation.CompanyId);
            await Clients.Group(groupName).SendAsync("ReceiveLocationUpdate", updatedLocation);
            
            _logger.LogDebug(
                "Location broadcast for vehicle {VehicleId} to group {GroupName}",
                locationDto.VehicleId,
                groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating location for vehicle {VehicleId}", locationDto.VehicleId);
            await Clients.Caller.SendAsync("ReceiveError", ex.Message);
        }
    }

    /// <summary>
    /// Request specific vehicle location
    /// </summary>
    public async Task RequestVehicleLocation(int vehicleId)
    {
        var location = await _locationService.GetLatestLocationAsync(vehicleId);
        if (location != null)
        {
            await Clients.Caller.SendAsync("ReceiveVehicleLocation", location);
        }
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    private static string GetCompanyGroupName(int companyId) => $"company_{companyId}";
}
