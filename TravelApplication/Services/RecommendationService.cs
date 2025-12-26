using System.Text.Json;
using TravelApplication.Models;

namespace TravelApplication.Services
{
    public class RecommendationService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://10.0.2.2:5217/api/recommendations";

        public RecommendationService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<PreferenceGroup>> GetTripRecommendationsAsync(int tripId)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{BaseUrl}/trip/{tripId}");
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var groups = JsonSerializer.Deserialize<List<PreferenceGroup>>(response, options);
                return groups ?? new List<PreferenceGroup>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching recommendations: {ex.Message}");
                return new List<PreferenceGroup>();
            }
        }
    }
}
