using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using wedeli.Service.Interface;

namespace wedeli.Service.Service
{
    /// <summary>
    /// Google Maps Geocoding service implementation
    /// </summary>
    public class GeocodingService : IGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GeocodingService> _logger;
        private const string GEOCODING_URL = "https://maps.googleapis.com/maps/api/geocode/json";

        public GeocodingService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GeocodingService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GoogleMaps:ApiKey"] 
                ?? throw new ArgumentNullException("GoogleMaps:ApiKey is not configured");
            _logger = logger;
        }

        /// <summary>
        /// Convert an address to geographic coordinates using Google Geocoding API
        /// </summary>
        public async Task<(decimal Latitude, decimal Longitude)?> GeocodeAddressAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                _logger.LogWarning("Empty address provided for geocoding");
                return null;
            }

            try
            {
                var encodedAddress = HttpUtility.UrlEncode(address);
                var requestUrl = $"{GEOCODING_URL}?address={encodedAddress}&key={_apiKey}";

                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GeocodeResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Status == "OK" && result.Results?.Length > 0)
                {
                    var location = result.Results[0].Geometry?.Location;
                    if (location != null)
                    {
                        _logger.LogInformation(
                            "Successfully geocoded address '{Address}' to ({Lat}, {Lng})",
                            address, location.Lat, location.Lng);
                        
                        return ((decimal)location.Lat, (decimal)location.Lng);
                    }
                }

                _logger.LogWarning(
                    "Geocoding failed for address '{Address}'. Status: {Status}",
                    address, result?.Status ?? "Unknown");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error geocoding address: {Address}", address);
                return null;
            }
        }

        #region Geocode Response Models
        private class GeocodeResponse
        {
            public string Status { get; set; }
            public GeocodeResult[] Results { get; set; }
        }

        private class GeocodeResult
        {
            public GeocodeGeometry Geometry { get; set; }
            public string FormattedAddress { get; set; }
        }

        private class GeocodeGeometry
        {
            public GeocodeLocation Location { get; set; }
        }

        private class GeocodeLocation
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }
        #endregion
    }
}
