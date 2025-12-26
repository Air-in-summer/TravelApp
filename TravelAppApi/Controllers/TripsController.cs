using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAppApi.Data;
using TravelAppApi.Models;
using TravelAppApi.Services;

namespace TravelAppApi.Controllers
{
    // API Controller quản lý các chuyến đi (Trips)
    // Cung cấp các endpoint: GET, POST, PUT, DELETE
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly TripService _tripService;

        public TripsController(ApplicationDbContext context, TripService tripService)
        {
            _context = context;
            _tripService = tripService;
        }

        // GET: api/Trips/user/{ownerId}
        // Lấy tất cả các chuyến đi của một user
        [HttpGet("user/{ownerId}")]
        public async Task<IActionResult> GetTrips(int ownerId)
        {
            try
            {
                var trip = await _tripService.GetTripsByUserIdAsync(ownerId);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi lấy thông tin chuyến đi: {ex.Message}");
            }
        }

        // GET: api/Trips/{id}
        // Lấy chi tiết một chuyến đi theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Trip>> GetTrip(int id)
        {
            try
            {
                var trip = await _tripService.GetTripsByIdAsync(id);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi lấy thông tin chuyến đi: {ex.Message}");
            }
        }

        
        // POST: api/Trips
        // Tạo một chuyến đi mới
        [HttpPost]
        public async Task<IActionResult> PostTrip(AddTripDto addTripDto)
        {           
            try
            {
                var tripModel = await _tripService.CreateTripAsync(addTripDto);
                return Ok(tripModel);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi tải lên: {ex.Message}");
            }
        }

        // PUT: api/Trips/{id}
        // Cập nhật thông tin chuyến đi
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTrip(int id, AddTripDto addTripDto)
        {            
            try
            {
                var trip = await _tripService.PutTripAsync(id, addTripDto);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi lấy thông tin chuyến đi: {ex.Message}");
            }
        }

        // DELETE: api/Trips/{id}
        // Xóa một chuyến đi
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrip(int id)
        {
            await _tripService.DeleteTripAsync(id);

            return NoContent();
        }

        
    }
}
