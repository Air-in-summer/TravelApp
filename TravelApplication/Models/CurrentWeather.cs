namespace TravelApplication.Models
{
    public class CurrentWeather
    {
        public double Temperature { get; set; }
        public int WeatherCode { get; set; }
        public double WindSpeed { get; set; }
        public int Humidity { get; set; }

        // Weather icon based on weather code
        public string WeatherIcon => GetWeatherIcon(WeatherCode);
        
        // Weather description
        public string Description => GetWeatherDescription(WeatherCode);

        private static string GetWeatherIcon(int code)
        {
            // Check if it's daytime (6 AM to 6 PM) based on current time
            bool isDaytime = DateTime.Now.Hour >= 6 && DateTime.Now.Hour < 18;
            
            return code switch
            {
                0 => isDaytime ? "☀️" : "🌙",  // Clear sky: sun or moon
                1 or 2 or 3 => isDaytime ? "⛅" : "☁️",  // Partly cloudy
                45 or 48 => "🌫️",            // Fog
                >= 51 and <= 67 => "🌧️",    // Rain
                >= 71 and <= 77 => "❄️",    // Snow
                >= 80 and <= 99 => "⛈️",    // Thunderstorm
                _ => "☁️"                     // Default cloudy
            };
        }

        private static string GetWeatherDescription(int code)
        {
            return code switch
            {
                0 => "Clear sky",
                1 => "Mainly clear",
                2 => "Partly cloudy",
                3 => "Overcast",
                45 or 48 => "Foggy",
                51 or 53 or 55 => "Drizzle",
                61 or 63 or 65 => "Rain",
                71 or 73 or 75 => "Snow",
                80 or 81 or 82 => "Rain showers",
                95 or 96 or 99 => "Thunderstorm",
                _ => "Cloudy"
            };
        }
    }
}
