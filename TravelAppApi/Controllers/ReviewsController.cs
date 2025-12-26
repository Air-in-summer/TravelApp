using Microsoft.AspNetCore.Mvc;
using TravelAppApi.Data;
using TravelAppApi.Services;

namespace TravelAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly ReviewService _reviewService;

        public ReviewsController(ReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // GET: api/reviews
        // Lấy TẤT CẢ reviews (cho tab "Review")
        [HttpGet]
        public async Task<IActionResult> GetAllReviews()
        {
            try
            {
                var reviews = await _reviewService.GetAllReviewsAsync();
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi lấy reviews: {ex.Message}");
            }
        }

        // GET: api/reviews/place/{placeId}
        // Lấy reviews theo địa điểm (cho search)
        [HttpGet("place/{placeId}")]
        public async Task<IActionResult> GetReviewsByPlace(string placeId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByPlaceIdAsync(placeId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi lấy reviews của địa điểm: {ex.Message}");
            }
        }

        // GET: api/reviews/my-reviews
        // Lấy reviews của user hiện tại (cho tab "Cá nhân")
        // TODO: Lấy userId từ JWT token khi có authentication
        [HttpGet("my-reviews")]
        public async Task<IActionResult> GetMyReviews([FromQuery] int userId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi lấy reviews của bạn: {ex.Message}");
            }
        }

        // POST: api/reviews
        // Tạo review mới
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] AddReviewDto dto, [FromQuery] int userId)
        {
            try
            {
                var review = await _reviewService.CreateReviewAsync(dto, userId);
                return CreatedAtAction(nameof(GetAllReviews), new { id = review.ReviewId }, review);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi tạo review: {ex.Message}");
            }
        }

        // PUT: api/reviews/{id}
        // Cập nhật review (chỉ owner)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto dto, [FromQuery] int userId)
        {
            try
            {
                var review = await _reviewService.UpdateReviewAsync(id, dto, userId);
                return Ok(review);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi cập nhật review: {ex.Message}");
            }
        }

        // DELETE: api/reviews/{id}
        // Xóa review (chỉ owner)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id, [FromQuery] int userId)
        {
            try
            {
                await _reviewService.DeleteReviewAsync(id, userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi xóa review: {ex.Message}");
            }
        }
    }
}
