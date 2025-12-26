using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TravelAppApi.Models
{
    [Table("Review")]
    public class Review
    {
        [Key, Column("reviewId")]
        public int ReviewId { get; set; }

        // Foreign Key
        [Column("userId")]
        public int UserId { get; set; }

        [Column("placeId")]
        public string PlaceId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("PlaceId")]
        public Places? Place { get; set; }
        
        [Column("rating")]
        public int Rating { get; set; }

        [Column("reviewText")]
        public string? ReviewText { get; set; } = string.Empty;

        [Column("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}
