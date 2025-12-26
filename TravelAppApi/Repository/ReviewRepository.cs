using Microsoft.EntityFrameworkCore;
using TravelAppApi.Data;
using TravelAppApi.Models;

namespace TravelAppApi.Repository
{
    public class ReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public ReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả reviews (cho tab "Review")
        public async Task<IEnumerable<Review>> GetAllReviewsAsync()
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Place)
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync();
        }

        // Lấy reviews theo PlaceId (cho search)
        public async Task<IEnumerable<Review>> GetReviewsByPlaceIdAsync(string placeId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Place)
                .Where(r => r.PlaceId == placeId)
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync();
        }

        // Lấy reviews của user (cho tab "Cá nhân")
        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Place)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync();
        }

        // Lấy 1 review theo ID
        public async Task<Review?> GetReviewByIdAsync(int reviewId)
        {
            return await _context.Reviews
                
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);
        }

        // Thêm review mới
        public async Task<Review> AddReviewAsync(Review review)
        {
            review.UpdatedAt = DateTime.UtcNow;
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        // Cập nhật review
        public async Task<Review> UpdateReviewAsync(Review review)
        {
            var existingReview = await _context.Reviews.FindAsync(review.ReviewId);
            if (existingReview != null)
            {
                existingReview.Rating = review.Rating;
                existingReview.ReviewText = review.ReviewText;
                existingReview.UpdatedAt = DateTime.UtcNow;
                
                _context.Reviews.Update(existingReview);
                await _context.SaveChangesAsync();
                return existingReview;
            }
            throw new Exception("Review not found");
        }

        // Xóa review
        public async Task DeleteReviewAsync(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
        }
    }
}
