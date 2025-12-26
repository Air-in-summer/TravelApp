using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TravelAppApi.Models
{
    [Table("User_Preferences")]
    public class UserPreferences
    {      
        [Key, Column("preferenceId")]
        public int PreferenceId { get; set; }

        // Foreign Key
        [Column("tripId")]
        public int TripId { get; set; }

        [ForeignKey(nameof(TripId))]
        public Trip? Trip { get; set; }

        [Required]
        [Column("preference")]
        public string? Preference { get; set; }        
    }
}
