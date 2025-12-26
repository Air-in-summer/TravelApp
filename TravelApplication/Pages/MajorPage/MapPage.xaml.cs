using Microsoft.Extensions.Configuration;
using TravelApplication.Models;
using TravelApplication.ViewModels;
using Mapsui.Projections;
using Mapsui.UI.Maui;
using Mapsui;
using CommunityToolkit.Mvvm.Input;

namespace TravelApplication.Pages.MajorPage;

public partial class MapPage : ContentPage
{
    private readonly MapViewModel viewModel;
    private int tripId;

    public MapPage(int tripId)
    {
        InitializeComponent();
        this.tripId = tripId;
        
        var configuration = ServiceHelper.GetService<IConfiguration>();
        viewModel = new MapViewModel(configuration, tripId);
        BindingContext = viewModel;

        // Disable MyLocationLayer to remove blue marker
        MapControl.MyLocationEnabled = false;
        MapControl.MyLocationFollow = false;

        // Wire up map tap event - Mapsui uses Info event with MapInfoEventArgs from Mapsui namespace
        MapControl.Info += OnMapInfo;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadTripAsync();
    }

    private async void OnMapInfo(object? sender, MapInfoEventArgs e)
    {
        // Manually unfocus SearchBar (MapView doesn't auto-claim focus)
        SearchBar1.Unfocus();
        
        // Clear search results when clicking on map
        viewModel.ClearSearchResults();
        
        if (e.MapInfo?.WorldPosition == null || viewModel.IsBusy)
            return;

        var worldPos = e.MapInfo.WorldPosition;
        
        // Convert from Spherical Mercator to Lat/Lon
        var (lon, lat) = SphericalMercator.ToLonLat(worldPos.X, worldPos.Y);

        // Reverse geocode and show popup
        var result = await viewModel.ReverseGeocodeAsync(lat, lon);
        if (result != null)
        {
            viewModel.ShowPlacePopup(result);
        }
    }

    private void OnSearchBarFocused(object? sender, FocusEventArgs e)
    {
        // Show search results when search bar is focused (if results exist)
        viewModel.ShowSearchResults();
    }

    private void OnSearchBarUnfocused(object? sender, FocusEventArgs e)
    {
        // Clear search results when search bar loses focus
        viewModel.ClearSearchResults();
    }

    private void OnSearchResultTapped(object? sender, TappedEventArgs e)
    {
        // Unfocus SearchBar when a search result is tapped
        SearchBar1.Unfocus();
        
        // Get the tapped result from the BindingContext
        if (sender is Border border && border.BindingContext is GeoapifyPlacesResult result)
        {
            viewModel.SelectSearchResult(result);
        }
    }

    private async void OnAddPlaceClicked(object sender, EventArgs e)
    {
        if (viewModel.SelectedPlace != null)
        {
            viewModel.ClosePlacePopup();
            await Navigation.PushAsync(new AddStopPage(tripId, viewModel.SelectedPlace));
        }
    }
}