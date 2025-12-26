namespace TravelApplication.Models
{
    public class WeatherData
    {
        public string PlaceName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public CurrentWeather? Current { get; set; }
        public List<HourlyForecast> Hourly { get; set; } = new();
        public List<DailyForecast> Daily { get; set; } = new();
    }
}
