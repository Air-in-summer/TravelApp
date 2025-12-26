namespace TravelAppApi.Data
{
    public class AddUserPreferencesDto
    {
        public int TripID { get; set; }
        public List<string> Preferences { get; set; } = new();
    }
}
