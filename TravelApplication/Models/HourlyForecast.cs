namespace TravelApplication.Models
{
    public class HourlyForecast
    {
        public DateTime Time { get; set; }
        public double Temperature { get; set; }
        public int WeatherCode { get; set; }
        public int PrecipitationProbability { get; set; }

        // Weather icon based on weather code and time of day
        public string WeatherIcon => GetWeatherIcon(WeatherCode, Time);
        
        // Hour label (e.g., "23:00")
        public string HourLabel => Time.ToString("HH:mm");

        private static string GetWeatherIcon(int code, DateTime time)
        {
            // Check if it's daytime (6 AM to 6 PM)
            bool isDaytime = time.Hour >= 6 && time.Hour < 18;
            
            return code switch
            {
                0 => isDaytime ? "☀️" : "🌙",  // Clear sky: sun or moon
                1 or 2 or 3 => isDaytime ? "⛅" : "☁️",  // Partly cloudy
                45 or 48 => "🌫️",
                >= 51 and <= 67 => "🌧️",
                >= 71 and <= 77 => "❄️",
                >= 80 and <= 99 => "⛈️",
                _ => "☁️"
            };
        }
    }
}
