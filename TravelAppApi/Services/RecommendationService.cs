using System.Text.Json;
using TravelAppApi.Data;

namespace TravelAppApi.Services
{
    public class RecommendationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        // Mapping preferences to Geoapify categories
        private static readonly Dictionary<string, string> PreferenceCategoryMap = new()
        {
            { "accomodation", "accommodation" },
            { "commercial", "commercial" },
            { "catering", "catering" },
            { "entertainment", "entertainment" },
            { "heritage", "tourism.sights" },
            { "leisure", "leisure" },
            { "natural", "natural" },
            { "national_park", "national_park" },
            { "religion", "religion" },
            { "camping", "camping" },
            { "beach", "natural.beach" }
        };


        public RecommendationService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["Geoapify:ApiKey"] ?? throw new Exception("Geoapify API key not found");
        }

        public async Task<List<PlaceRecommendation>> GetRecommendationsAsync(
            List<string> preferences,
            double minLat, double minLon,
            double maxLat, double maxLon)
        {
            var allRecommendations = new List<PlaceRecommendation>();
            var centerLat = (minLat + maxLat) / 2;
            var centerLon = (minLon + maxLon) / 2;

            // Query each preference in parallel
            var tasks = preferences.Select(preference =>
                QueryGeoapifyPlacesAsync(preference, minLat, minLon, maxLat, maxLon, centerLat, centerLon)
            );

            var results = await Task.WhenAll(tasks);

            foreach (var result in results)
            {
                allRecommendations.AddRange(result);
            }

            // Deduplicate by PlaceId, keep first occurrence
            var uniquePlaces = allRecommendations
                .GroupBy(p => p.PlaceId)
                .Select(g => g.First())
                .OrderBy(p => p.Distance)
                .ToList();

            return uniquePlaces;
        }

        private async Task<List<PlaceRecommendation>> QueryGeoapifyPlacesAsync(
            string preference,
            double minLat, double minLon,
            double maxLat, double maxLon,
            double centerLat, double centerLon,
            int limit = 15)
        {
            try
            {
                var category = MapPreferenceToCategory(preference);
                
                var url = $"https://api.geoapify.com/v2/places" +
                    $"?categories={category}" +
                    $"&filter=rect:{minLon},{minLat},{maxLon},{maxLat}" +
                    $"&bias=proximity:{centerLon},{centerLat}" +
                    $"&limit={limit}" +
                    $"&apiKey={_apiKey}";

                var response = await _httpClient.GetStringAsync(url);
                var jsonDoc = JsonDocument.Parse(response);

                var places = new List<PlaceRecommendation>();

                if (jsonDoc.RootElement.TryGetProperty("features", out var features))
                {
                    foreach (var feature in features.EnumerateArray())
                    {
                        var properties = feature.GetProperty("properties");
                        var geometry = feature.GetProperty("geometry");
                        var coordinates = geometry.GetProperty("coordinates");

                        var lon = coordinates[0].GetDouble();
                        var lat = coordinates[1].GetDouble();

                        places.Add(new PlaceRecommendation
                        {
                            Preference = preference,
                            PlaceId = properties.TryGetProperty("place_id", out var placeId) 
                                ? placeId.GetString() ?? Guid.NewGuid().ToString() 
                                : Guid.NewGuid().ToString(),
                            Name = properties.TryGetProperty("name", out var name) 
                                ? name.GetString() ?? "Unknown" 
                                : "Unknown",
                            Category = properties.TryGetProperty("categories", out var cats) 
                                ? cats.EnumerateArray().FirstOrDefault().GetString() ?? category 
                                : category,
                            Latitude = lat,
                            Longitude = lon,
                            Address = properties.TryGetProperty("formatted", out var addr) 
                                ? addr.GetString() ?? "" 
                                : "",
                            Distance = CalculateDistance(centerLat, centerLon, lat, lon)
                        });
                    }
                }

                return places;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error querying Geoapify for {preference}: {ex.Message}");
                return new List<PlaceRecommendation>();
            }
        }

        private string MapPreferenceToCategory(string preference)
        {
            return PreferenceCategoryMap.GetValueOrDefault(preference, "tourism");
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Haversine formula
            const double R = 6371; // Earth radius in km

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
