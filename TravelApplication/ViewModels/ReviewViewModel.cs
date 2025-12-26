using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TravelApplication.Models;
using TravelApplication.Services;

namespace TravelApplication.ViewModels
{
    public partial class ReviewViewModel : BaseViewModel
    {
        private readonly ReviewService _reviewService;

        [ObservableProperty]
        ObservableCollection<Review> allReviews = new();

        [ObservableProperty]
        ObservableCollection<Review> myReviews = new();

        [ObservableProperty]
        ObservableCollection<Review> filteredReviews = new();

        [ObservableProperty]
        string searchQuery = string.Empty;

        [ObservableProperty]
        int selectedTabIndex = 0; // 0 = Review, 1 = Cá nhân

        [ObservableProperty]
        bool isRefreshing;

        public ReviewViewModel()
        {
            _reviewService = ServiceHelper.GetService<ReviewService>();
            Title = "Reviews";
        }

        // Load tất cả reviews (cho tab "Review")
        [RelayCommand]
        async Task LoadAllReviewsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var reviews = await _reviewService.GetAllReviewsAsync();
                AllReviews.Clear();
                foreach (var review in reviews)
                {
                    AllReviews.Add(review);
                }
                FilteredReviews = new ObservableCollection<Review>(AllReviews);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load reviews: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        // Load reviews của mình (cho tab "Cá nhân")
        [RelayCommand]
        async Task LoadMyReviewsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var reviews = await _reviewService.GetMyReviewsAsync();
                MyReviews.Clear();
                foreach (var review in reviews)
                {
                    MyReviews.Add(review);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load your reviews: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        // Search reviews theo địa điểm
        [RelayCommand]
        async Task SearchReviewsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                FilteredReviews = new ObservableCollection<Review>(AllReviews);
                return;
            }

            try
            {
                IsBusy = true;
                // Search by place name OR address (destination/city)
                var filtered = AllReviews.Where(r => 
                    r.PlaceName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    r.Address.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)
                ).ToList();

                FilteredReviews = new ObservableCollection<Review>(filtered);
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

        // Clear search
        [RelayCommand]
        void ClearSearch()
        {
            SearchQuery = string.Empty;
            FilteredReviews = new ObservableCollection<Review>(AllReviews);
        }

        // Navigate to AddReviewPage
        [RelayCommand]
        async Task AddReviewAsync()
        {
            await Shell.Current.Navigation.PushAsync(new Pages.AddReviewPage());
        }

        // Navigate to AddReviewPage (Edit mode)
        [RelayCommand]
        async Task EditReviewAsync(Review review)
        {
            if (review == null) return;
            await Shell.Current.Navigation.PushAsync(new Pages.AddReviewPage(review));
        }

        // Delete review
        [RelayCommand]
        async Task DeleteReviewAsync(Review review)
        {
            if (review == null) return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Delete Review",
                "Are you sure you want to delete this review?",
                "Delete",
                "Cancel");

            if (!confirm) return;

            try
            {
                IsBusy = true;
                await _reviewService.DeleteReviewAsync(review.ReviewId);
                
                // Remove from MyReviews
                MyReviews.Remove(review);
                
                // Also remove from AllReviews if exists
                var reviewInAll = AllReviews.FirstOrDefault(r => r.ReviewId == review.ReviewId);
                if (reviewInAll != null)
                {
                    AllReviews.Remove(reviewInAll);
                }

                await Shell.Current.DisplayAlert("Success", "Review deleted successfully", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to delete review: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Tab changed
        [RelayCommand]
        async Task TabChangedAsync(int tabIndex)
        {
            SelectedTabIndex = tabIndex;
            
            if (tabIndex == 0) // Tab "Review"
            {
                await LoadAllReviewsAsync();
            }
            else if (tabIndex == 1) // Tab "Cá nhân"
            {
                await LoadMyReviewsAsync();
            }
        }
    }
}
