using System.Collections.ObjectModel;

namespace TravelApplication.Models
{
    public class PlaceRecommendation
    {
        public string Preference { get; set; } = string.Empty;
        public string PlaceId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public double Distance { get; set; }

        public string DistanceText => $"{Distance:F1} km";

        // Convert to GeoapifyPlacesResult for AddStopPage
        public GeoapifyPlacesResult ToGeoapifyPlacesResult()
        {
            return new GeoapifyPlacesResult
            {
                Location = Name,
                LocationId = PlaceId,
                Category = Category,
                Latitude = Latitude,
                Longitude = Longitude,
                Address = Address
            };
        }
    }

    public class PreferenceGroup
    {
        public string Preference { get; set; } = string.Empty;
        public ObservableCollection<PlaceRecommendation> Places { get; set; } = new();
    }
}
