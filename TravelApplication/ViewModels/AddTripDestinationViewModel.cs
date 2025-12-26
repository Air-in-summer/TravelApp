using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using TravelApplication.Models;
using TravelApplication.Pages.MajorPage;

namespace TravelApplication.ViewModels
{
    public partial class AddTripDestinationViewModel : BaseViewModel
    {
        private readonly HttpClient httpClient = new();
        private readonly string apiKey;
        public ObservableCollection<GeoapifyResult> Results { get; set; } = new();
        public string SearchQuery { get; set; }

        public ICommand SearchCommand { get; }
        public AddTripDestinationViewModel(IConfiguration configuration)
        {

            SearchCommand = new Command(async () => await SearchAsync());
            apiKey = configuration["Geoapify:ApiKey"];

        }

        private async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery) || SearchQuery.Length < 3) return;

            var url = $"https://api.geoapify.com/v1/geocode/search?text={Uri.EscapeDataString(SearchQuery)}&apiKey={apiKey}";

            try
            {
                var response = await httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(response);

                var features = doc.RootElement.GetProperty("features");
                
                Results.Clear();

                foreach (var feature in features.EnumerateArray())
                {
                    var props = feature.GetProperty("properties");

                    var city = props.TryGetProperty("city", out var cityProp) ? cityProp.GetString() : null;
                    var country = props.TryGetProperty("country", out var countryProp) ? countryProp.GetString() : null;
                    
                    var placeId = props.GetProperty("place_id").GetString();

                    // LẤY BBOX
                    double minLon = 0, minLat = 0, maxLon = 0, maxLat = 0;
                    //var bbox = feature.GetProperty("bbox");
                    if (feature.TryGetProperty("bbox", out var bboxProp) && bboxProp.ValueKind == JsonValueKind.Array)
                    {
                        minLon = bboxProp[0].GetDouble();
                        minLat = bboxProp[1].GetDouble();
                        maxLon = bboxProp[2].GetDouble();
                        maxLat = bboxProp[3].GetDouble();
                    }

                    if (!string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(country))
                    {
                        Results.Add(new GeoapifyResult
                        {
                            City = city,
                            Country = country,
                            PlaceId = placeId,
                            MinLon = minLon,
                            MinLat = minLat,
                            MaxLon = maxLon,
                            MaxLat = maxLat
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                string innerMessage = ex.InnerException?.Message ?? ex.Message;
                await Shell.Current.DisplayAlert("Search Error", $"An error occurred while searching for destinations.\n\nError: {innerMessage}", "OK");
            }
        }

        
    }
}
