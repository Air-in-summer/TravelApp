using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Windows.Input;
using TravelApplication.Models;
using System.Collections.ObjectModel;
using TravelApplication.Services;

namespace TravelApplication.ViewModels
{
    public partial class AddStopDestinationViewModel : BaseViewModel
    {
        private readonly HttpClient httpClient = new();
        private readonly TripService tripService;
        private readonly string apiKey;

        public ObservableCollection<GeoapifyPlacesResult> Results { get; set; } = new();

        public string SearchQuery { get; set; }

        public ICommand SearchCommand { get; }
        int TripId { get; set; }
        public AddStopDestinationViewModel(IConfiguration configuration, int tripId)
        {
            TripId = tripId;
            SearchCommand = new Command(async () => await SearchAsync());
            apiKey = configuration["Geoapify:ApiKey"];
            tripService = ServiceHelper.GetService<TripService>();
        }

        private async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery) || SearchQuery.Trim().Length < 2)
            {
                Results.Clear();
                return;
            }

            // 🔹 BƯỚC 1: LẤY THÔNG TIN TRIP ĐỂ CÓ BBOX
            // → BẠN CẦN THAY THẾ PHẦN NÀY BẰNG GỌI SERVICE/REPOSITORY THẬT
            var trip = await tripService.GetTripAsync(TripId); // 👈 bạn cần cài đặt hàm này

            if (trip == null)
            {
                await Shell.Current.DisplayAlert("Error", "Trip not found.", "OK");
                return;
            }

            // 🔹 BƯỚC 2: XÂY DỰNG RECT FILTER TỪ BBOX
            var rectFilter = $"{trip.MinLon},{trip.MinLat},{trip.MaxLon},{trip.MaxLat}";

            // 🔹 BƯỚC 3: GỌI GEOAPIFY AUTOCOMPLETE VỚI FILTER RECT
            var encodedQuery = Uri.EscapeDataString(SearchQuery.Trim());
            var url = $"https://api.geoapify.com/v1/geocode/autocomplete?text={encodedQuery}&filter=rect:{rectFilter}&apiKey={apiKey}";

            try
            {
                var response = await httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(response);

                Results.Clear();

                // Kiểm tra và lấy features
                if (!doc.RootElement.TryGetProperty("features", out var features) ||
                    features.ValueKind != JsonValueKind.Array)
                {
                    return;
                }

                foreach (var feature in features.EnumerateArray())
                {
                    if (!feature.TryGetProperty("properties", out var props))
                        continue;

                    // Bắt buộc phải có lat, lon
                    if (!props.TryGetProperty("lat", out var latProp) ||
                        !props.TryGetProperty("lon", out var lonProp))
                        continue;

                    var lat = latProp.GetDouble();
                    var lon = lonProp.GetDouble();

                    var formatted = props.TryGetProperty("name", out var f) ? f.GetString() : null;
                    var placeId = props.TryGetProperty("place_id", out var pid) ? pid.GetString() : null;
                    var category = props.TryGetProperty("category", out var cat) ? cat.GetString() : "unknown";
                    var address = props.TryGetProperty("address_line2", out var addr) ? addr.GetString() : null;
                    
                    // Bỏ qua nếu không có tên hoặc place_id
                    if (string.IsNullOrWhiteSpace(formatted) || string.IsNullOrWhiteSpace(placeId))
                        continue;

                    // 🔹 FILTER: Chỉ giữ places có category hợp lệ
                    // Normalize category để kiểm tra
                    //var normalizedCategory = NormalizeCategory(category ?? "unknown");
                    
                    // Loại bỏ nếu category không hợp lệ (other/unknown)
                    if (category == "unknown")
                        continue; // Thà không có còn hơn sai

                    Results.Add(new GeoapifyPlacesResult
                    {
                        Location = formatted,
                        LocationId = placeId,
                        Category = category ?? "unknown",
                        Latitude = lat,
                        Longitude = lon,
                        Address = address ?? string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                await Shell.Current.DisplayAlert("Search Error", $"Failed to search places.\n{msg}", "OK");
            }
        }
    }
}
