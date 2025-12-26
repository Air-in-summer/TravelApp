using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelAppApi.Models
{
    [Table("Trips")]
    public class Trip
    {
        [Key]
        [Column("tripId")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column("destinationId")]
        public string DestinationId { get; set; } = string.Empty;
        
        [Required]
        [Column("startDate")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("endDate")]
        public DateTime EndDate { get; set; }

        [Column("budget")]
        public decimal Budget { get; set; }

        // Foreign key to User
        [Column("userId")]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [ForeignKey(nameof(DestinationId))]
        public Destinations? Destination { get; set; }

        public ICollection<Stop> Stops { get; set; } = new List<Stop>();
        public ICollection<UserPreferences>? Preferences { get; set; }
    }
}
