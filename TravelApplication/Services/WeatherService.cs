using System.Text.Json;
using TravelApplication.Models;

namespace TravelApplication.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.open-meteo.com/v1/forecast";

        public WeatherService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<WeatherData?> GetWeatherAsync(double latitude, double longitude, string placeName)
        {
            try
            {
                // Build API URL
                var url = $"{BaseUrl}?" +
                    $"latitude={latitude}&" +
                    $"longitude={longitude}&" +
                    $"current=temperature_2m,weathercode,windspeed_10m,relativehumidity_2m&" +
                    $"hourly=temperature_2m,weathercode,precipitation_probability&" +
                    $"daily=temperature_2m_max,temperature_2m_min,weathercode,precipitation_sum&" +
                    $"timezone=auto";

                var response = await _httpClient.GetStringAsync(url);
                var jsonDoc = JsonDocument.Parse(response);
                var root = jsonDoc.RootElement;

                var weatherData = new WeatherData
                {
                    PlaceName = placeName,
                    Latitude = latitude,
                    Longitude = longitude
                };

                // Parse current weather
                if (root.TryGetProperty("current", out var current))
                {
                    weatherData.Current = new CurrentWeather
                    {
                        Temperature = current.GetProperty("temperature_2m").GetDouble(),
                        WeatherCode = current.GetProperty("weathercode").GetInt32(),
                        WindSpeed = current.GetProperty("windspeed_10m").GetDouble(),
                        Humidity = current.GetProperty("relativehumidity_2m").GetInt32()
                    };
                }

                // Parse hourly forecast (next 24 hours)
                if (root.TryGetProperty("hourly", out var hourly))
                {
                    var times = hourly.GetProperty("time").EnumerateArray().Select(t => DateTime.Parse(t.GetString()!)).ToList();
                    var temps = hourly.GetProperty("temperature_2m").EnumerateArray().Select(t => t.GetDouble()).ToList();
                    var codes = hourly.GetProperty("weathercode").EnumerateArray().Select(c => c.GetInt32()).ToList();
                    var precips = hourly.GetProperty("precipitation_probability").EnumerateArray().Select(p => p.GetInt32()).ToList();

                    var now = DateTime.Now;
                    for (int i = 0; i < times.Count && weatherData.Hourly.Count < 24; i++)
                    {
                        if (times[i] >= now)
                        {
                            weatherData.Hourly.Add(new HourlyForecast
                            {
                                Time = times[i],
                                Temperature = temps[i],
                                WeatherCode = codes[i],
                                PrecipitationProbability = precips[i]
                            });
                        }
                    }
                }

                // Parse daily forecast (7 days)
                if (root.TryGetProperty("daily", out var daily))
                {
                    var dates = daily.GetProperty("time").EnumerateArray().Select(d => DateTime.Parse(d.GetString()!)).ToList();
                    var maxTemps = daily.GetProperty("temperature_2m_max").EnumerateArray().Select(t => t.GetDouble()).ToList();
                    var minTemps = daily.GetProperty("temperature_2m_min").EnumerateArray().Select(t => t.GetDouble()).ToList();
                    var codes = daily.GetProperty("weathercode").EnumerateArray().Select(c => c.GetInt32()).ToList();
                    var precips = daily.GetProperty("precipitation_sum").EnumerateArray().Select(p => p.GetDouble()).ToList();

                    for (int i = 0; i < Math.Min(7, dates.Count); i++)
                    {
                        weatherData.Daily.Add(new DailyForecast
                        {
                            Date = dates[i],
                            MaxTemperature = maxTemps[i],
                            MinTemperature = minTemps[i],
                            WeatherCode = codes[i],
                            PrecipitationSum = precips[i]
                        });
                    }
                }

                return weatherData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching weather: {ex.Message}");
                return null;
            }
        }
    }
}
