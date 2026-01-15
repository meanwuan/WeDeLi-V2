using System.Threading.Tasks;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Geocoding service interface for converting addresses to coordinates
    /// </summary>
    public interface IGeocodingService
    {
        /// <summary>
        /// Convert an address to geographic coordinates
        /// </summary>
        /// <param name="address">The address to geocode</param>
        /// <returns>Tuple of (Latitude, Longitude) or null if geocoding fails</returns>
        Task<(decimal Latitude, decimal Longitude)?> GeocodeAddressAsync(string address);
    }
}
