namespace TravelApplication.Models
{
    public class DailyForecast
    {
        public DateTime Date { get; set; }
        public double MaxTemperature { get; set; }
        public double MinTemperature { get; set; }
        public int WeatherCode { get; set; }
        public double PrecipitationSum { get; set; }

        // Weather icon based on weather code
        public string WeatherIcon => GetWeatherIcon(WeatherCode);
        
        // Day label (e.g., "Mon", "Tue")
        public string DayLabel => Date.ToString("ddd");
        
        // Full date label (e.g., "Dec 15")
        public string DateLabel => Date.ToString("MMM dd");

        private static string GetWeatherIcon(int code)
        {
            return code switch
            {
                0 => "☀️",
                1 or 2 or 3 => "⛅",
                45 or 48 => "🌫️",
                >= 51 and <= 67 => "🌧️",
                >= 71 and <= 77 => "❄️",
                >= 80 and <= 99 => "⛈️",
                _ => "☁️"
            };
        }
    }
}
