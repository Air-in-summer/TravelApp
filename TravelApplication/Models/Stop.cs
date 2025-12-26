using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelApplication.Models
{
    public class Stop
    {
        public int Id { get; set; }
        public string Location { get; set; } = string.Empty;
        public string LocationId { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public DateTime ArrivalDate { get; set; } = DateTime.Now;
        public DateTime DepartureDate { get; set; } = DateTime.Now;
        public decimal EstimatedCost { get; set; }
        public string Notes { get; set; } = string.Empty;
        public int TripId { get; set; }
    }
}
