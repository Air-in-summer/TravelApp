using Microsoft.AspNetCore.Mvc;
using TravelAppApi.Data;
using TravelAppApi.Repository;
using TravelAppApi.Services;

namespace TravelAppApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationsController : ControllerBase
    {
        private readonly RecommendationService _recommendationService;
        private readonly UserPreferencesRepository _preferencesRepo;
        private readonly TripRepository _tripRepo;

        public RecommendationsController(
            RecommendationService recommendationService,
            UserPreferencesRepository preferencesRepo,
            TripRepository tripRepo)
        {
            _recommendationService = recommendationService;
            _preferencesRepo = preferencesRepo;
            _tripRepo = tripRepo;
        }

        [HttpGet("trip/{tripId}")]
        public async Task<IActionResult> GetTripRecommendations(int tripId)
        {
            try
            {
                // 1. Get trip preferences
                var preferences = await _preferencesRepo.GetUserPreferencesAsync(tripId);
                if (preferences.Count() != 3)
                {
                    return BadRequest("Trip must have exactly 3 preferences");
                }

                var preferenceList = preferences.Select(p => p.Preference!).ToList();

                // 2. Get trip bounding box
                var trip = await _tripRepo.GetTripByIdAsync(tripId);
                if (trip == null)
                {
                    return NotFound("Trip not found");
                }

                // 3. Get recommendations
                var recommendations = await _recommendationService.GetRecommendationsAsync(
                    preferenceList,
                    trip.Destination.MinLatitude, trip.Destination.MinLongitude,
                    trip.Destination.MaxLatitude, trip.Destination.MaxLongitude
                );

                // 4. Group by preference
                var grouped = preferenceList.Select(pref => new PreferenceGroupDto
                {
                    Preference = pref,
                    Places = recommendations
                        .Where(r => r.Preference == pref)
                        .Take(15) // Limit to 15 per preference
                        .ToList()
                }).ToList();

                return Ok(grouped);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting recommendations: {ex.Message}");
            }
        }
    }
}
