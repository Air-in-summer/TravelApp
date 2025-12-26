using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TravelApplication.Models;
using TravelApplication.Services;

namespace TravelApplication.ViewModels
{
    public partial class AddReviewViewModel : BaseViewModel
    {
        private readonly ReviewService _reviewService;
        private readonly GeoService _geoService;

        [ObservableProperty]
        Review currentReview = new();

        [ObservableProperty]
        int selectedRating = 0;

        [ObservableProperty]
        Review? reviewToEdit;

        [ObservableProperty]
        string searchQuery = string.Empty;

        [ObservableProperty]
        ObservableCollection<GeoapifyPlacesResult> searchResults = new();

        [ObservableProperty]
        bool isSearchResultsVisible;

        public bool IsEditMode => reviewToEdit != null;

        // Constructor cho Add mode
        public AddReviewViewModel()
        {
            _reviewService = ServiceHelper.GetService<ReviewService>();
            _geoService = ServiceHelper.GetService<GeoService>();
            Title = "Add Review";
        }

        // Constructor cho Edit mode
        public AddReviewViewModel(Review review)
        {
            _reviewService = ServiceHelper.GetService<ReviewService>();
            _geoService = ServiceHelper.GetService<GeoService>();
            reviewToEdit = review;
            Title = "Edit Review";
            
            // Load dữ liệu hiện tại
            CurrentReview = new Review
            {
                ReviewId = review.ReviewId,
                PlaceId = review.PlaceId,
                PlaceName = review.PlaceName,
                Category = review.Category,
                Latitude = review.Latitude,
                Longitude = review.Longitude,
                Address = review.Address,
                Rating = review.Rating,
                ReviewText = review.ReviewText
            };
            SelectedRating = review.Rating;
        }

        // Search places (giống MapPage)
        [RelayCommand]
        async Task SearchPlacesAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                SearchResults.Clear();
                IsSearchResultsVisible = false;
                return;
            }

            try
            {
                IsBusy = true;
                var results = await _geoService.GetPlacesAsync(SearchQuery);
                SearchResults.Clear();
                foreach (var result in results)
                {
                    SearchResults.Add(result);
                }
                IsSearchResultsVisible = SearchResults.Count > 0;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Search failed: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Select place from search results
        [RelayCommand]
        void SelectPlace(GeoapifyPlacesResult place)
        {
            if (place == null) return;

            // Cập nhật CurrentReview với thông tin place
            CurrentReview.PlaceId = place.LocationId;
            CurrentReview.PlaceName = place.Location;
            CurrentReview.Category = place.Category;
            CurrentReview.Latitude = place.Latitude;
            CurrentReview.Longitude = place.Longitude;
            CurrentReview.Address = place.Address;

            // Clear search
            SearchQuery = string.Empty;
            SearchResults.Clear();
            IsSearchResultsVisible = false;

            OnPropertyChanged(nameof(CurrentReview));
        }

        // Set rating (1-5)
        [RelayCommand]
        void SetRating(int rating)
        {
            if (rating < 1 || rating > 5) return;
            SelectedRating = rating;
            CurrentReview.Rating = rating;
        }

        // Save review
        [RelayCommand]
        async Task SaveReviewAsync()
        {
            // Validate
            if (string.IsNullOrWhiteSpace(CurrentReview.PlaceId) && !IsEditMode)
            {
                await Shell.Current.DisplayAlert("Error", "Please select a place", "OK");
                return;
            }

            if (SelectedRating < 1 || SelectedRating > 5)
            {
                await Shell.Current.DisplayAlert("Error", "Please select a rating (1-5 stars)", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentReview.ReviewText))
            {
                await Shell.Current.DisplayAlert("Error", "Please write a review", "OK");
                return;
            }

            try
            {
                IsBusy = true;

                if (IsEditMode)
                {
                    // Update existing review
                    await _reviewService.UpdateReviewAsync(reviewToEdit!.ReviewId, CurrentReview);
                    await Shell.Current.DisplayAlert("Success", "Review updated successfully", "OK");
                }
                else
                {
                    // Create new review

                    await _reviewService.AddReviewAsync(CurrentReview);
                    await Shell.Current.DisplayAlert("Success", "Review added successfully", "OK");
                }

                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to save review: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
