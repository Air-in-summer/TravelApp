using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAppApi.Data;
using TravelAppApi.Models;
using TravelAppApi.Repository;
namespace TravelAppApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PreferencesController : ControllerBase
    {
        private readonly UserPreferencesRepository _userPreferencesRepository;
        public PreferencesController(UserPreferencesRepository userPreferencesRepository)
        {
            _userPreferencesRepository = userPreferencesRepository;
        }

        [HttpPost]
        public async Task<IActionResult> SaveUserPreferences([FromBody] AddUserPreferencesDto dto)
        {
            if (dto.Preferences.Count != 3)
                return BadRequest("Cần chọn đúng 3 sở thích.");

            var existingPrefs = await _userPreferencesRepository.GetUserPreferencesAsync(dto.TripID);
            foreach (var pref in existingPrefs)
            {
                Console.WriteLine($"Existing preference: {pref.Preference}");
            }
            if (existingPrefs.Count() > 0)
            {            
                Console.WriteLine("Existing preferences found, updating them.");
                await _userPreferencesRepository.UpdateUserPreferencesAsync(dto.TripID, dto.Preferences, existingPrefs);
            }
            else
            {               
                Console.WriteLine("No existing preferences found, adding new ones.");
                await _userPreferencesRepository.AddUserPreferencesAsync(dto.TripID, dto.Preferences);
            }
            

            return Ok(new { message = "Lưu thành công" });
        }

        [HttpGet("{tripId}")]
        public async Task<IActionResult> GetUserPreferences(int tripId)
        {
            try
            {
                var prefs = await _userPreferencesRepository.GetUserPreferencesAsync(tripId);
                return Ok(prefs);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi lấy thông tin sở thích người dùng: {ex.Message}");
            }
        }
    }

}
