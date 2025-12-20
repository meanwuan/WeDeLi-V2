namespace wedeli.Models.DTO.Vehicle;

/// <summary>
/// DTO cho response vị trí xe
/// </summary>
public class VehicleLocationDto
{
    public int VehicleId { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? Speed { get; set; }
    public decimal? Heading { get; set; }
    public decimal? Accuracy { get; set; }
    public string Status { get; set; } = "Moving";
    public DateTime RecordedAt { get; set; }
    
    // Additional vehicle info
    public int CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public string? CurrentStatus { get; set; }
    public int? DriverId { get; set; }
    public string? DriverName { get; set; }
}

/// <summary>
/// DTO cho request cập nhật vị trí từ driver app
/// </summary>
public class UpdateVehicleLocationDto
{
    public int VehicleId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? Speed { get; set; }
    public decimal? Heading { get; set; }
    public decimal? Accuracy { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// DTO cho danh sách vị trí xe của công ty
/// </summary>
public class CompanyVehicleLocationsDto
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int TotalVehicles { get; set; }
    public int OnlineVehicles { get; set; }
    public List<VehicleLocationDto> Vehicles { get; set; } = new();
}
