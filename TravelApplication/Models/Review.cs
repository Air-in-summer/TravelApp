namespace TravelApplication.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PlaceId { get; set; } = string.Empty;
        public string PlaceName { get; set; } = string.Empty;
        public int Rating { get; set; }  // 1-5
        public string? ReviewText { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Thông tin đầy đủ về Place (để gửi lên backend khi tạo review)
        public string Category { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
    }
}
