using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TravelApplication.Models;
using TravelApplication.Services;

namespace TravelApplication.ViewModels
{
    public partial class PreferenceRecommendationsViewModel : BaseViewModel
    {
        private readonly RecommendationService _recommendationService;

        [ObservableProperty]
        int tripId;

        public ObservableCollection<PreferenceGroup> PreferenceGroups { get; } = new();

        [ObservableProperty]
        string errorMessage = string.Empty;

        public PreferenceRecommendationsViewModel(int tripId)
        {
            _recommendationService = ServiceHelper.GetService<RecommendationService>();
            TripId = tripId;
            Title = "Recommendations for You";

            _ = LoadRecommendationsAsync();
        }

        [RelayCommand]
        async Task LoadRecommendationsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var groups = await _recommendationService.GetTripRecommendationsAsync(TripId);

                PreferenceGroups.Clear();
                foreach (var group in groups)
                {
                    PreferenceGroups.Add(group);
                }

                if (PreferenceGroups.Count == 0)
                {
                    ErrorMessage = "No recommendations found. Make sure your trip has 3 preferences set.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load recommendations: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task AddToTripAsync(PlaceRecommendation place)
        {
            try
            {
                // Convert to GeoapifyPlacesResult
                var geoapifyResult = place.ToGeoapifyPlacesResult();

                // Navigate to AddStopPage with correct constructor
                await Shell.Current.Navigation.PushAsync(
                    new Pages.MajorPage.AddStopPage(TripId, geoapifyResult)
                );
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to add: {ex.Message}", "OK");
            }
        }
    }
}
