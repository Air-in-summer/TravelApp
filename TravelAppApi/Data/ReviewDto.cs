namespace TravelAppApi.Data
{
    // DTO để thêm review mới
    public class AddReviewDto
    {
        public string UserName { get; set; } = string.Empty;
        public string PlaceId { get; set; } = string.Empty;
        public string PlaceName { get; set; } = string.Empty;
        public int Rating { get; set; }  // 1-5
        public string? ReviewText { get; set; }
        
        // Thông tin đầy đủ về Place (để tạo Place nếu chưa tồn tại)
        
        public string Category { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
    }

    // DTO để cập nhật review
    public class UpdateReviewDto
    {
        public int Rating { get; set; }  // 1-5
        public string? ReviewText { get; set; }
    }

    // DTO trả về cho frontend
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PlaceId { get; set; } = string.Empty;
        public string PlaceName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? ReviewText { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        
        public string Category { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
    }

}
