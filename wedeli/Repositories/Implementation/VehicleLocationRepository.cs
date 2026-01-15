using Microsoft.EntityFrameworkCore;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Implementation;

public class VehicleLocationRepository : IVehicleLocationRepository
{
    private readonly AppDbContext _context;

    public VehicleLocationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<VehicleLocation?> GetLatestByVehicleIdAsync(int vehicleId)
    {
        return await _context.VehicleLocations
            .Include(v => v.Vehicle)
                .ThenInclude(v => v.Company)
            .Where(v => v.VehicleId == vehicleId)
            .OrderByDescending(v => v.RecordedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<VehicleLocation>> GetLatestByCompanyIdAsync(int companyId)
    {
        // Lấy vị trí mới nhất của mỗi xe trong công ty
        var latestLocations = await _context.VehicleLocations
            .Include(v => v.Vehicle)
                .ThenInclude(v => v.Company)
            .Where(v => v.Vehicle.CompanyId == companyId)
            .GroupBy(v => v.VehicleId)
            .Select(g => g.OrderByDescending(v => v.RecordedAt).First())
            .ToListAsync();

        return latestLocations;
    }

    public async Task<VehicleLocation> AddAsync(VehicleLocation location)
    {
        location.CreatedAt = DateTime.UtcNow;
        _context.VehicleLocations.Add(location);
        await _context.SaveChangesAsync();
        return location;
    }

    public async Task<VehicleLocation> UpsertAsync(VehicleLocation location)
    {
        // Tìm record gần nhất trong vòng 30 giây
        var existing = await _context.VehicleLocations
            .Where(v => v.VehicleId == location.VehicleId 
                && v.RecordedAt > DateTime.UtcNow.AddSeconds(-30))
            .OrderByDescending(v => v.RecordedAt)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            // Update existing record
            existing.Latitude = location.Latitude;
            existing.Longitude = location.Longitude;
            existing.Speed = location.Speed;
            existing.Heading = location.Heading;
            existing.Accuracy = location.Accuracy;
            existing.Status = location.Status;
            existing.RecordedAt = location.RecordedAt;
            
            _context.VehicleLocations.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }
        else
        {
            // Add new record
            return await AddAsync(location);
        }
    }

    public async Task<IEnumerable<VehicleLocation>> GetHistoryAsync(int vehicleId, DateTime from, DateTime to)
    {
        return await _context.VehicleLocations
            .Where(v => v.VehicleId == vehicleId 
                && v.RecordedAt >= from 
                && v.RecordedAt <= to)
            .OrderBy(v => v.RecordedAt)
            .ToListAsync();
    }

    public async Task<int> DeleteOldRecordsAsync(DateTime before)
    {
        var oldRecords = await _context.VehicleLocations
            .Where(v => v.RecordedAt < before)
            .ToListAsync();

        _context.VehicleLocations.RemoveRange(oldRecords);
        return await _context.SaveChangesAsync();
    }
}
