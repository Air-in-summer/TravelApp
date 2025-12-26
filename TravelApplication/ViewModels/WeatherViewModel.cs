using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TravelApplication.Models;
using TravelApplication.Services;

namespace TravelApplication.ViewModels
{
    public partial class WeatherViewModel : BaseViewModel
    {
        private readonly WeatherService _weatherService;

        [ObservableProperty]
        double latitude;

        [ObservableProperty]
        double longitude;

        [ObservableProperty]
        string placeName = string.Empty;

        [ObservableProperty]
        WeatherData? currentWeather;

        public ObservableCollection<HourlyForecast> HourlyForecasts { get; } = new();
        public ObservableCollection<DailyForecast> DailyForecasts { get; } = new();

        [ObservableProperty]
        string errorMessage = string.Empty;

        public WeatherViewModel()
        {
            _weatherService = ServiceHelper.GetService<WeatherService>();
            Title = "Weather Forecast";
        }

        // Constructor với parameters (cho PushAsync)
        public WeatherViewModel(double latitude, double longitude, string placeName)
        {
            _weatherService = ServiceHelper.GetService<WeatherService>();
            Title = "Weather Forecast";
            
            Latitude = latitude;
            Longitude = longitude;
            PlaceName = placeName;
            
            // Auto load weather
            _ = LoadWeatherAsync();
        }

        [RelayCommand]
        async Task LoadWeatherAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var weatherData = await _weatherService.GetWeatherAsync(Latitude, Longitude, PlaceName);

                if (weatherData != null)
                {
                    CurrentWeather = weatherData;

                    HourlyForecasts.Clear();
                    foreach (var hourly in weatherData.Hourly)
                    {
                        HourlyForecasts.Add(hourly);
                    }

                    DailyForecasts.Clear();
                    foreach (var daily in weatherData.Daily)
                    {
                        DailyForecasts.Add(daily);
                    }
                }
                else
                {
                    ErrorMessage = "Failed to load weather data. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
