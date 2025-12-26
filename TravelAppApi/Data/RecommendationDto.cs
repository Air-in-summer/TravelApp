namespace TravelAppApi.Data
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
        public double Distance { get; set; } // km from center
    }

    public class PreferenceGroupDto
    {
        public string Preference { get; set; } = string.Empty;
        public List<PlaceRecommendation> Places { get; set; } = new();
    }
}
