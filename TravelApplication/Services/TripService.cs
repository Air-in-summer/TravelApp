using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Text.Json;
using TravelApplication.Models;

namespace TravelApplication.Services
{
    public interface ITripService
    {
        // Trip methods
        Task<List<Trip>> GetTripsAsync();
        Task<Trip> GetTripAsync(int id);
        Task<Trip> AddTripAsync(Trip trip);
        Task<Trip> UpdateTripAsync(Trip trip);
        Task DeleteTripAsync(int id);
        Task AddPreferencesAsync(int tripId, List<string> preferences);
    }
    public class TripService : ITripService
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions serializerOptions;

        public TripService(HttpClient _httpClient)
        {
            this.httpClient = _httpClient;
            serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private async Task HandleErrors(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                // This will throw an exception with the detailed error from the API
                throw new HttpRequestException(errorContent, null, response.StatusCode);
            }
        }

        // Get trip by user
        public async Task<List<Trip>> GetTripsAsync()
        {
            string userIdstr = SecureStorage.GetAsync("userID").Result;
            int userId = int.Parse(userIdstr);
            var response = await httpClient.GetAsync($"api/trips/user/{userId}");
            await HandleErrors(response);
            return await response.Content.ReadFromJsonAsync<List<Trip>>(serializerOptions) ?? new();
        }

        public async Task<Trip> GetTripAsync(int id)
        {
            var response = await httpClient.GetAsync($"api/trips/{id}");
            await HandleErrors(response);
            return await response.Content.ReadFromJsonAsync<Trip>(serializerOptions) ?? new();
        }

        public async Task<Trip> AddTripAsync(Trip trip)
        {
            string userIdstr = SecureStorage.GetAsync("userID").Result;
            int userId = int.Parse(userIdstr);
            trip.UserId = userId;
            var response = await httpClient.PostAsJsonAsync("api/trips", trip);
            await HandleErrors(response);
            
            return await response.Content.ReadFromJsonAsync<Trip>() ?? new();
        }

        public async Task<Trip> UpdateTripAsync(Trip trip)
        {
            string userIdstr = SecureStorage.GetAsync("userID").Result;
            int userId = int.Parse(userIdstr);
            trip.UserId = userId;
            var response = await httpClient.PutAsJsonAsync($"api/trips/{trip.Id}", trip);
            await HandleErrors(response);

            return await response.Content.ReadFromJsonAsync<Trip>() ?? new();
        }

        public async Task DeleteTripAsync(int id)
        {
            var response = await httpClient.DeleteAsync($"api/trips/{id}");
            await HandleErrors(response);
        }

        public async Task AddPreferencesAsync(int tripId, List<string> preferences)
        {
            var dto = new
            {
                TripID = tripId,
                Preferences = preferences
            };
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("api/preferences", content);
            await HandleErrors(response);
        }

    }
}
