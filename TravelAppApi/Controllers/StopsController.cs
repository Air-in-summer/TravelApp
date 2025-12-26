using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAppApi.Data;
using TravelAppApi.Models;
using TravelAppApi.Services;
namespace TravelAppApi.Controllers
{
    // API Controller quản lý các điểm dừng (Stops) trong chuyến đi
    [Route("api/[controller]")]
    [ApiController]
    public class StopsController : ControllerBase
    {
        private readonly StopService _stopService;

        public StopsController(StopService stopService)
        {
            _stopService = stopService;
        }

        // GET: api/Stops/ByTrip/{tripId}
        // Lấy tất cả các điểm dừng của một chuyến đi
        [HttpGet("ByTrip/{tripId}")]
        public async Task<ActionResult<IEnumerable<Stop>>> GetStopsForTrip(int tripId)
        {
            try
            {
                var stops = await _stopService.GetStopsByTripIdAsync(tripId);
                return Ok(stops);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi lấy thông tin điểm dừng: {ex.Message}");
            }
        }

        // GET: api/Stops/{id}
        // Lấy thông tin chi tiết một điểm dừng
        [HttpGet("{id}")]
        public async Task<ActionResult<Stop>> GetStop(int id)
        {
            try
            {
                var stop = await _stopService.GetStopByIdAsync(id);
                return Ok(stop);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi lấy thông tin điểm dừng: {ex.Message}");
            }
        }

        // POST: api/Stops
        // Tạo một điểm dừng mới
        [HttpPost]
        public async Task<ActionResult<Stop>> PostStop(AddStopDto addStopDto)
        {
            try
            {
                var stopModel = await _stopService.CreateStopAsync(addStopDto);
                return Ok(stopModel);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi tải lên: {ex.Message}");
            }
        }

        // PUT: api/Stops/{id}
        // Cập nhật thông tin điểm dừng
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStop(int id, AddStopDto addStopDto)
        {
            try
            {
                var stop = await _stopService.PutStopAsync(id, addStopDto);
                return Ok(stop);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi lấy thông tin chuyến đi: {ex.Message}");
            }

        }

        // DELETE: api/Stops/{id}
        // Xóa một điểm dừng
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStop(int id)
        {
            await _stopService.DeleteStopAsync(id);

            return NoContent();
        }

        
    }
}
