using System.Net.Http;
using System.Text.Json;
using TravelApplication.Models;
using Microsoft.Extensions.Configuration;

namespace TravelApplication.Services
{
    public class GeoService
    {
        private readonly HttpClient httpClient;
        private readonly string apiKey;

        public GeoService(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            apiKey = configuration["Geoapify:ApiKey"];
        }

        // Search places using Geoapify Autocomplete API
        public async Task<List<GeoapifyPlacesResult>> GetPlacesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<GeoapifyPlacesResult>();

            try
            {
                var encodedQuery = Uri.EscapeDataString(query.Trim());
                var url = $"https://api.geoapify.com/v1/geocode/autocomplete?text={encodedQuery}&apiKey={apiKey}&limit=10";

                var response = await httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(response);

                var results = new List<GeoapifyPlacesResult>();

                if (!doc.RootElement.TryGetProperty("features", out var features) ||
                    features.ValueKind != JsonValueKind.Array)
                {
                    return results;
                }

                foreach (var feature in features.EnumerateArray())
                {
                    if (!feature.TryGetProperty("properties", out var props))
                        continue;

                    if (!props.TryGetProperty("lat", out var latProp) ||
                        !props.TryGetProperty("lon", out var lonProp))
                        continue;

                    var lat = latProp.GetDouble();
                    var lon = lonProp.GetDouble();

                    var displayName = props.TryGetProperty("name", out var f) ? f.GetString() : null;
                    var locationId = props.TryGetProperty("place_id", out var pid) ? pid.GetString() : null;
                    var category = props.TryGetProperty("category", out var cat) ? cat.GetString() : "unknown";
                    var address = props.TryGetProperty("address_line2", out var addr) ? addr.GetString() : null;

                    if (string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(locationId))
                        continue;

                    results.Add(new GeoapifyPlacesResult
                    {
                        Location = displayName,
                        LocationId = locationId,
                        Category = category,
                        Latitude = lat,
                        Longitude = lon,
                        Address = address ?? string.Empty
                    });
                }

                return results;
            }
            catch (Exception ex)
            {
                // Log error if needed
                return new List<GeoapifyPlacesResult>();
            }
        }
    }
}
