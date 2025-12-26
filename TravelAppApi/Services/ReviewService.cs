using TravelAppApi.Data;
using TravelAppApi.Models;
using TravelAppApi.Repository;

namespace TravelAppApi.Services
{
    public class ReviewService
    {
        private readonly ReviewRepository _reviewRepository;
        private readonly PlaceRepository _placeRepository;

        public ReviewService(ReviewRepository reviewRepository, PlaceRepository placeRepository)
        {
            _reviewRepository = reviewRepository;
            _placeRepository = placeRepository;
        }

        // Lấy tất cả reviews (cho tab "Review")
        public async Task<IEnumerable<ReviewDto>> GetAllReviewsAsync()
        {
            var reviews = await _reviewRepository.GetAllReviewsAsync();
            return reviews.Select(r => MapToDto(r));
        }

        // Lấy reviews theo PlaceId (cho search)
        public async Task<IEnumerable<ReviewDto>> GetReviewsByPlaceIdAsync(string placeId)
        {
            var reviews = await _reviewRepository.GetReviewsByPlaceIdAsync(placeId);
            return reviews.Select(r => MapToDto(r));
        }

        // Lấy reviews của user (cho tab "Cá nhân")
        public async Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(int userId)
        {
            var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
            return reviews.Select(r => MapToDto(r));
        }

        // Tạo review mới
        public async Task<ReviewDto> CreateReviewAsync(AddReviewDto dto, int userId)
        {
            // Validate rating
            if (dto.Rating < 1 || dto.Rating > 5)
            {
                throw new ArgumentException("Rating must be between 1 and 5");
            }

            // Validate review text length
            if (!string.IsNullOrEmpty(dto.ReviewText) && dto.ReviewText.Length > 1000)
            {
                throw new ArgumentException("Review text must not exceed 1000 characters");
            }

            // Kiểm tra user đã review place này chưa
            var existingReviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
            var alreadyReviewed = existingReviews.Any(r => r.PlaceId == dto.PlaceId);
            if (alreadyReviewed)
            {
                throw new InvalidOperationException("You have already reviewed this place. Please edit your existing review instead.");
            }

            // Kiểm tra Place đã tồn tại chưa
            var existingPlace = await _placeRepository.GetPlaceAsync(dto.PlaceId);
            if (existingPlace == null)
            {
                // Tạo Place mới nếu chưa tồn tại
                var newPlace = new Places
                {
                    Id = dto.PlaceId,
                    Name = dto.PlaceName,
                    Category = dto.Category,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    FormattedAddress = dto.Address
                };
                await _placeRepository.AddPlaceAsync(newPlace);
            }

            var review = new Review
            {
                UserId = userId,
                PlaceId = dto.PlaceId,
                Rating = dto.Rating,
                ReviewText = dto.ReviewText,
                UpdatedAt = DateTime.UtcNow
            };

            var createdReview = await _reviewRepository.AddReviewAsync(review);
            
            // Reload để lấy User và Place info
            var reviewWithDetails = await _reviewRepository.GetReviewByIdAsync(createdReview.ReviewId);
            
            return MapToDto(reviewWithDetails!);
        }

        // Cập nhật review
        public async Task<ReviewDto> UpdateReviewAsync(int reviewId, UpdateReviewDto dto, int userId)
        {
            // Validate rating
            if (dto.Rating < 1 || dto.Rating > 5)
            {
                throw new ArgumentException("Rating must be between 1 and 5");
            }

            // Validate review text length
            if (!string.IsNullOrEmpty(dto.ReviewText) && dto.ReviewText.Length > 1000)
            {
                throw new ArgumentException("Review text must not exceed 1000 characters");
            }

            // Kiểm tra review tồn tại
            var existingReview = await _reviewRepository.GetReviewByIdAsync(reviewId);
            if (existingReview == null)
            {
                throw new Exception("Review not found");
            }

            // Kiểm tra ownership
            if (existingReview.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can only edit your own reviews");
            }

            existingReview.Rating = dto.Rating;
            existingReview.ReviewText = dto.ReviewText;

            var updatedReview = await _reviewRepository.UpdateReviewAsync(existingReview);
            
            // Reload để lấy User và Place info
            var reviewWithDetails = await _reviewRepository.GetReviewByIdAsync(updatedReview.ReviewId);
            return MapToDto(reviewWithDetails!);
        }

        // Xóa review
        public async Task DeleteReviewAsync(int reviewId, int userId)
        {
            // Kiểm tra review tồn tại
            var existingReview = await _reviewRepository.GetReviewByIdAsync(reviewId);
            if (existingReview == null)
            {
                throw new Exception("Review not found");
            }

            // Kiểm tra ownership
            if (existingReview.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can only delete your own reviews");
            }

            await _reviewRepository.DeleteReviewAsync(reviewId);
        }

        // Helper method: Map Review entity to ReviewDto
        private ReviewDto MapToDto(Review review)
        {

            return new ReviewDto
            {
                ReviewId = review.ReviewId,
                UserId = review.UserId,
                UserName = review.User?.Username ?? "Unknown",
                PlaceId = review.PlaceId,
                PlaceName = review.Place?.Name ?? "Unknown Place",
                Rating = review.Rating,
                ReviewText = review.ReviewText,
                UpdatedAt = review.UpdatedAt,
                // Thông tin đầy đủ về Place
                Category = review.Place?.Category ?? string.Empty,
                Latitude = review.Place?.Latitude ?? 0,
                Longitude = review.Place?.Longitude ?? 0,
                Address = review.Place?.FormattedAddress ?? string.Empty
            };
        }

        
    }
}
