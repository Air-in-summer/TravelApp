using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelAppApi.Models
{
    [Table("Stops")]
    public class Stop
    {
        [Key]
        [Column("stopId")]
        public int Id { get; set; }

        [Required]
        [Column("locationId")]
        public string LocationId { get; set; } = string.Empty;

        [Required]
        [Column("arrivalDate")]
        public DateTime ArrivalDate { get; set; }

        [Required]
        [Column("departureDate")]
        public DateTime DepartureDate { get; set; }

        [Column("estimatedCost")]
        public decimal EstimatedCost { get; set; }

        [Column("notes")]
        public string Notes { get; set; } = string.Empty;

        // Foreign Key
        [Column("tripId")]
        public int TripId { get; set; }

        [ForeignKey(nameof(TripId))]
        public Trip? Trip { get; set; }

        [ForeignKey(nameof(LocationId))]
        public Places? Location { get; set; }
    }
}
