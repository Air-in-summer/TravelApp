using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TravelAppApi.Models
{
    [Table("Places")]
    public class Places
    {
        [Key]
        [Column("placeId")]
        public string Id { get; set; } = string.Empty;

        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("latitude")]   
        public double Latitude { get; set; }

        [Required]
        [Column("longitude")]
        public double Longitude { get; set; }

        [Required]
        [Column("formattedAddress")]
        public string FormattedAddress { get; set; } = string.Empty;

        [Required]
        [Column("category")]
        public string Category { get; set; } = string.Empty;

        public ICollection<Stop>? Stop { get; set; }

        public ICollection<Review>? Review { get; set; }
    }
}
