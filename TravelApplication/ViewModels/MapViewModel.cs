using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using Mapsui.UI.Maui;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TravelApplication.Models;
using TravelApplication.Services;
using static SkiaSharp.HarfBuzz.SKShaper;
using Font = Mapsui.Styles.Font;

namespace TravelApplication.ViewModels
{
    public partial class MapViewModel : BaseViewModel
    {
        private readonly TripService tripService;
        private readonly HttpClient httpClient = new();
        private readonly string apiKey;

        [ObservableProperty]
        Trip currentTrip = default!;

        public ObservableCollection<Stop> Stops { get; } = new();

        [ObservableProperty]
        string searchQuery = string.Empty;

        [ObservableProperty]
        ObservableCollection<GeoapifyPlacesResult> searchResults = new();

        [ObservableProperty]
        GeoapifyPlacesResult? selectedPlace;

        [ObservableProperty]
        bool isPlacePopupVisible;

        [ObservableProperty]
        public bool isSearchResultsVisible; //=> SearchResults.Count > 0;

        public Mapsui.Map Map { get; set; } = new();

        private ILayer? stopsLayer;

        private int tripId;

        public MapViewModel(IConfiguration configuration, int tripId)
        {
            this.tripId = tripId;
            tripService = ServiceHelper.GetService<TripService>();
            apiKey = configuration["Geoapify:ApiKey"];
            
            InitializeMap();
        }

        private void InitializeMap()
        {
            // Create a tile layer using OpenStreetMap
            Map.Layers.Add(OpenStreetMap.CreateTileLayer());

            // Create a writable layer for stops (simpler than MemoryProvider)
            stopsLayer = new WritableLayer
            {
                Name = "Stops",
                Style = null // We'll set styles on individual features
            };
            Map.Layers.Add(stopsLayer);
            
            // DON'T set initial position here - let LoadTripAsync handle it
            // to avoid race condition with CenterMapToDestinationAsync
        }

        public async Task LoadTripAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                CurrentTrip = await tripService.GetTripAsync(tripId);
                if (CurrentTrip == null)
                {
                    await Shell.Current.DisplayAlert("Error", "Trip not found.", "OK");
                    return;
                }

                Stops.Clear();
                
                // CLEAR ALL LAYERS to remove any unwanted markers (including blue marker)
                Map.Layers.Clear();
                
                // Re-add tile layer
                Map.Layers.Add(OpenStreetMap.CreateTileLayer());
                
                // Re-create stops layer
                stopsLayer = new WritableLayer
                {
                    Name = "Stops",
                    Style = null
                };
                Map.Layers.Add(stopsLayer);
                
                // Add stop markers
                if (CurrentTrip.Stops != null && CurrentTrip.Stops.Any())
                {
                    foreach (var stop in CurrentTrip.Stops.OrderBy(s => s.ArrivalDate))
                    {
                        Stops.Add(stop);
                        AddStopMarker(stop);
                    }
                }

                // Center and zoom map (includes RefreshData)
                await CenterMapToDestinationAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load trip: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // hàm này để thêm marker vào map - cải thiện để hiển thị rõ hơn
        private void AddStopMarker(Stop stop)
        {
            if (stopsLayer is not WritableLayer writableLayer)
                return;

            // IMPORTANT: Convert lat/lon to Spherical Mercator (Web Mercator) for Mapsui
            var (x, y) = SphericalMercator.FromLonLat(stop.Longitude, stop.Latitude);
            var point = new MPoint(x, y);
            
            var feature = new PointFeature(point);
            
            

            // Create a visible marker style
            feature.Styles.Add(new SymbolStyle
            {
                SymbolType = SymbolType.Ellipse,
                Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.FromArgb(255, 220, 53, 69)), // Red color
                Outline = new Pen(Mapsui.Styles.Color.FromArgb(255, 139, 0, 0), 3), // Dark red outline
                SymbolScale = 0.8,  // Scale for the symbol
                SymbolOffset = new Offset(0, 0)
            });

            // Add label style to show stop name
            feature.Styles.Add(new LabelStyle
            {
                Text = stop.Location,
                BackColor = new Mapsui.Styles.Brush(Mapsui.Styles.Color.FromArgb(230, 255, 255, 255)),
                ForeColor = Mapsui.Styles.Color.Black,
                Offset = new Offset(0, -25),
                Font = new Font { FontFamily = "Arial", Size = 11, Bold = true },
                Halo = new Pen(Mapsui.Styles.Color.White, 2),
                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center
            });

            // Add feature to the writable layer
            writableLayer.Add(feature);
        }
        // hàm này để căn chỉnh map vào điểm đến
        private async Task CenterMapToDestinationAsync()
        {
            if (CurrentTrip == null)
            {
                System.Diagnostics.Debug.WriteLine("WARNING: CenterMapToDestinationAsync called but CurrentTrip is null!");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"=== CenterMapToDestinationAsync ===");
            System.Diagnostics.Debug.WriteLine($"Trip bounds: Lat({CurrentTrip.MinLat:F4}, {CurrentTrip.MaxLat:F4}), Lon({CurrentTrip.MinLon:F4}, {CurrentTrip.MaxLon:F4})");

            // Calculate center point of destination
            var centerLat = (CurrentTrip.MinLat + CurrentTrip.MaxLat) / 2.0;
            var centerLon = (CurrentTrip.MinLon + CurrentTrip.MaxLon) / 2.0;
            
            System.Diagnostics.Debug.WriteLine($"Center: ({centerLat:F4}, {centerLon:F4})");

            // Convert to Spherical Mercator
            var (x, y) = SphericalMercator.FromLonLat(centerLon, centerLat);
            System.Diagnostics.Debug.WriteLine($"Spherical Mercator: ({x:F2}, {y:F2})");

            // Calculate appropriate zoom level based on bounds
            var latDiff = CurrentTrip.MaxLat - CurrentTrip.MinLat;
            var lonDiff = CurrentTrip.MaxLon - CurrentTrip.MinLon;
            var maxDiff = Math.Max(latDiff, lonDiff);
            
            // Calculate appropriate resolution based on bounds
            double resolution = 156543.04; // meters per pixel at zoom level 0
            if (maxDiff > 5) resolution = 156543.04 / Math.Pow(2, 6);
            else if (maxDiff > 2) resolution = 156543.04 / Math.Pow(2, 8);
            else if (maxDiff > 0.5) resolution = 156543.04 / Math.Pow(2, 10);
            else if (maxDiff > 0.1) resolution = 156543.04 / Math.Pow(2, 12);
            else resolution = 156543.04 / Math.Pow(2, 14);

            System.Diagnostics.Debug.WriteLine($"maxDiff: {maxDiff:F4}, resolution: {resolution:F2}");

            // Small delay to ensure map is ready
            await Task.Delay(150);
            
            // Use CenterOnAndZoomTo for atomic operation (center + zoom together)
            Map.Navigator.CenterOnAndZoomTo(new MPoint(x, y), resolution, 300);
            
            // Refresh map after zoom
            await Task.Delay(350); // Wait for animation to complete
            Map.RefreshData(ChangeType.Discrete);
            
            System.Diagnostics.Debug.WriteLine($"=== Map centered and zoomed ===");
        }

        [RelayCommand]
        public async Task SearchPlacesAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery) || SearchQuery.Trim().Length < 2)
            {
                SearchResults.Clear();
                OnPropertyChanged(nameof(IsSearchResultsVisible));
                return;
            }

            if (CurrentTrip == null)
            {
                await Shell.Current.DisplayAlert("Error", "Trip not loaded.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var rectFilter = $"{CurrentTrip.MinLon},{CurrentTrip.MinLat},{CurrentTrip.MaxLon},{CurrentTrip.MaxLat}";
                var encodedQuery = Uri.EscapeDataString(SearchQuery.Trim());
                var url = $"https://api.geoapify.com/v1/geocode/autocomplete?text={encodedQuery}&filter=rect:{rectFilter}&apiKey={apiKey}&limit=10";

                var response = await httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(response);

                SearchResults.Clear();

                if (!doc.RootElement.TryGetProperty("features", out var features) ||
                    features.ValueKind != JsonValueKind.Array)
                {
                    return;
                }

                foreach (var feature in features.EnumerateArray())
                {
                    if (!feature.TryGetProperty("properties", out var props))
                        continue;

                    if (!props.TryGetProperty("lat", out var latProp) ||
                        !props.TryGetProperty("lon", out var lonProp))
                        continue;

                    var lat = latProp.GetDouble();
                    var lon = lonProp.GetDouble();

                    var formatted = props.TryGetProperty("name", out var f) ? f.GetString() : null;
                    var placeId = props.TryGetProperty("place_id", out var pid) ? pid.GetString() : null;
                    var category = props.TryGetProperty("category", out var cat) ? cat.GetString() : "unknown";
                    
                    var address = props.TryGetProperty("address_line2", out var addr) ? addr.GetString() : null;

                    if (string.IsNullOrWhiteSpace(formatted) || string.IsNullOrWhiteSpace(placeId))
                        continue;

                    // 🔹 FILTER: Loại bỏ places không có category hợp lệ
                    if (category == "unknown")
                        continue; 

                    SearchResults.Add(new GeoapifyPlacesResult
                    {
                        Location = formatted,
                        LocationId = placeId,
                        Category = category ?? "unknown",
                        Latitude = lat,
                        Longitude = lon,
                        Address = address ?? string.Empty
                    });
                }

                OnPropertyChanged(nameof(IsSearchResultsVisible));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Search Error", $"Failed to search places: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
                IsSearchResultsVisible = true;
            }   
        }

        [RelayCommand]
        public void SelectSearchResult(GeoapifyPlacesResult result)
        {
            if (result == null) return;

            SelectedPlace = result;
            IsPlacePopupVisible = true;
            IsSearchResultsVisible = false;
            // Zoom to the selected location
            var (x, y) = SphericalMercator.FromLonLat(result.Longitude, result.Latitude);
            Map.Navigator.CenterOn(x, y);
            Map.Navigator.ZoomTo(2);
        }

        [RelayCommand]
        public void ClosePlacePopup()
        {
            IsPlacePopupVisible = false;
            //IsSearchResultsVisible = true;
            //SelectedPlace = null;
        }

        public void ClearSearchResults()
        {
            //SearchResults.Clear();
            
            IsSearchResultsVisible = false;
            OnPropertyChanged(nameof(IsSearchResultsVisible));
        }

        public void ShowSearchResults()
        {
            // Show search results if they exist
            if (SearchResults.Count > 0)
            {
                IsSearchResultsVisible = true;
                OnPropertyChanged(nameof(IsSearchResultsVisible));
            }
        }

        [RelayCommand]
        public void ShowPlacePopup(GeoapifyPlacesResult place)
        {
            SelectedPlace = place;
            IsSearchResultsVisible = false;
            IsPlacePopupVisible = true;
        }

        public async Task<GeoapifyPlacesResult?> ReverseGeocodeAsync(double lat, double lon)
        {
            if (CurrentTrip == null) return null;

            // Validate coordinates are within trip destination bounds
            if (lat < CurrentTrip.MinLat || lat > CurrentTrip.MaxLat ||
                lon < CurrentTrip.MinLon || lon > CurrentTrip.MaxLon)
            {
                await Shell.Current.DisplayAlert("Location Error", 
                    "Selected location is outside the trip destination area.", "OK");
                return null;
            }

            IsBusy = true;
            try
            {
                var url = $"https://api.geoapify.com/v1/geocode/reverse?lat={lat}&lon={lon}&apiKey={apiKey}";

                var response = await httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(response);

                if (!doc.RootElement.TryGetProperty("features", out var features) ||
                    features.ValueKind != JsonValueKind.Array ||
                    features.GetArrayLength() == 0)
                {
                    await Shell.Current.DisplayAlert("Error", "Could not find location information.", "OK");
                    return null;
                }

                var firstFeature = features[0];
                if (!firstFeature.TryGetProperty("properties", out var props))
                    return null;

                var placeId = props.TryGetProperty("place_id", out var pid) ? pid.GetString() : null;
                var name = props.TryGetProperty("name", out var n) ? n.GetString() : 
                          props.TryGetProperty("formatted", out var f) ? f.GetString() : null;
                var category = props.TryGetProperty("category", out var cat) ? cat.GetString() : "unknown";
                
                // Try to get address from multiple possible fields
                string address = string.Empty;
                if (props.TryGetProperty("formatted", out var fmt))
                    address = fmt.GetString() ?? string.Empty;
                else if (props.TryGetProperty("address_line2", out var addr))
                    address = addr.GetString() ?? string.Empty;
                else if (props.TryGetProperty("address_line1", out var addr1))
                    address = addr1.GetString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(placeId))
                {
                    await Shell.Current.DisplayAlert("Error", "Incomplete location information.", "OK");
                    return null;
                }

                return new GeoapifyPlacesResult
                {
                    Location = name,
                    LocationId = placeId,
                    Category = category ?? "unknown",
                    Latitude = lat,
                    Longitude = lon,
                    Address = address ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to get location information: {ex.Message}", "OK");
                return null;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}