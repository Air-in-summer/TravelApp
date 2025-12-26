using System.Net.Http.Json;
using System.Text.Json;
using TravelApplication.Models;

namespace TravelApplication.Services
{
    public class ReviewService
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions serializerOptions;

        public ReviewService(HttpClient _httpClient)
        {
            this.httpClient = _httpClient;
            serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // Lấy tất cả reviews (cho tab "Review")
        public async Task<List<Review>> GetAllReviewsAsync()
        {
            var response = await httpClient.GetAsync("api/reviews");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Review>>(content, serializerOptions) ?? new List<Review>();
            }
            throw new Exception(await response.Content.ReadAsStringAsync());
        }

        // Lấy reviews theo PlaceId (cho search)
        public async Task<List<Review>> GetReviewsByPlaceAsync(string placeId)
        {
            var response = await httpClient.GetAsync($"api/reviews/place/{placeId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Review>>(content, serializerOptions) ?? new List<Review>();
            }
            throw new Exception(await response.Content.ReadAsStringAsync());
        }

        // Lấy reviews của user hiện tại (cho tab "Cá nhân")
        public async Task<List<Review>> GetMyReviewsAsync()
        {
            var userId = await SecureStorage.GetAsync("userID");
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("User not logged in");
            }

            var response = await httpClient.GetAsync($"api/reviews/my-reviews?userId={userId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Review>>(content, serializerOptions) ?? new List<Review>();
            }
            throw new Exception(await response.Content.ReadAsStringAsync());
        }

        // Thêm review mới
        public async Task<Review> AddReviewAsync(Review review)
        {
            var userId = await SecureStorage.GetAsync("userID");
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("User not logged in");
            }

            var addReviewDto = new
            {
                PlaceId = review.PlaceId,
                PlaceName = review.PlaceName,
                Rating = review.Rating,
                ReviewText = review.ReviewText,
                Category = review.Category,
                Latitude = review.Latitude,
                Longitude = review.Longitude,
                Address = review.Address
            };

            var response = await httpClient.PostAsJsonAsync($"api/reviews?userId={userId}", addReviewDto);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Review>(content, serializerOptions)!;
            }
            throw new Exception(await response.Content.ReadAsStringAsync());
        }

        // Cập nhật review
        public async Task<Review> UpdateReviewAsync(int reviewId, Review review)
        {
            var userId = await SecureStorage.GetAsync("userID");
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("User not logged in");
            }

            var updateReviewDto = new
            {
                Rating = review.Rating,
                ReviewText = review.ReviewText
            };

            var response = await httpClient.PutAsJsonAsync($"api/reviews/{reviewId}?userId={userId}", updateReviewDto);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Review>(content, serializerOptions)!;
            }
            throw new Exception(await response.Content.ReadAsStringAsync());
        }

        // Xóa review
        public async Task DeleteReviewAsync(int reviewId)
        {
            var userId = await SecureStorage.GetAsync("userID");
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("User not logged in");
            }

            var response = await httpClient.DeleteAsync($"api/reviews/{reviewId}?userId={userId}");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }
    }
}
