using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TravelApplication.Models;
using System.Net.Http.Json;

namespace TravelApplication.Services
{
    public interface IStopService
    {
        // Stop methods
        Task<List<Stop>> GetStopsForTripAsync(int tripId);
        Task<Stop> AddStopAsync(Stop stop);
        Task UpdateStopAsync(Stop stop);
        Task DeleteStopAsync(int id);
    }
    public class StopService : IStopService 
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions serializerOptions;

        public StopService(HttpClient _httpClient)
        {
            this.httpClient = _httpClient;
            //httpClient = ServiceHelper.GetService<HttpClient>();
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
        public async Task<List<Stop>> GetStopsForTripAsync(int tripId)
        {
            var response = await httpClient.GetAsync($"api/stops/ByTrip/{tripId}");
            await HandleErrors(response);
            return await response.Content.ReadFromJsonAsync<List<Stop>>(serializerOptions) ?? new();
        }

        public async Task<Stop> AddStopAsync(Stop stop)
        {
            var response = await httpClient.PostAsJsonAsync("api/stops", stop);
            await HandleErrors(response); // Now uses the new error handler
            return await response.Content.ReadFromJsonAsync<Stop>() ?? new();
        }

        public async Task UpdateStopAsync(Stop stop)
        {
            var response = await httpClient.PutAsJsonAsync($"api/stops/{stop.Id}", stop);
            await HandleErrors(response);
        }

        public async Task DeleteStopAsync(int id)
        {
            var response = await httpClient.DeleteAsync($"api/stops/{id}");
            await HandleErrors(response);
        }
    }
}
