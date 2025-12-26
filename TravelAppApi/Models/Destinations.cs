using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TravelAppApi.Models
{
    [Table("Destinations")]
    public class Destinations
    {
        [Key]
        [Column("destinationId")]
        public string Id { get; set; } = string.Empty;

        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("minLatitude")]
        public double MinLatitude { get; set; }

        [Required]
        [Column("minLongitude")]
        public double MinLongitude { get; set; }

        [Required]
        [Column("maxLatitude")]
        public double MaxLatitude { get; set; }

        [Required]
        [Column("maxLongitude")]
        public double MaxLongitude { get; set; }

        public ICollection<Trip>? Trips { get; set; }
    }
}
