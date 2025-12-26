using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelApplication.Models
{
    public class GeoapifyResult
    {
        public string City { get; set; }
        public string Country { get; set; }
        public string DisplayName => $"{City}, {Country}";
        
        public string PlaceId { get; set; }
        public double MinLon { get; set; }
        public double MinLat { get; set; }
        public double MaxLon { get; set; }
        public double MaxLat { get; set; }
    }

    public class GeoapifyPlacesResult 
    {
        public string Location { get; set; } = string.Empty;      // formatted address or POI name
        public string LocationId { get; set; } = string.Empty;    // place_id
        public string Category { get; set; } = "unknown";         // e.g., "entertainment.zoo"
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;

        public string DisplayName => Location;
    }

}
