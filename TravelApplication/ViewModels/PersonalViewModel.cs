using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TravelApplication.Models;
using TravelApplication.Services;

namespace TravelApplication.ViewModels
{
    public partial class PersonalViewModel : BaseViewModel
    {
        private readonly ReviewService _reviewService;

        [ObservableProperty]
        ObservableCollection<Review> myReviews = new();

        [ObservableProperty]
        bool isRefreshing;

        [ObservableProperty]
        string userName = "User";

        [ObservableProperty]
        string email = "user@example.com";

        public PersonalViewModel()
        {
            _reviewService = ServiceHelper.GetService<ReviewService>();
            Title = "Cá nhân";
            
            // Load user info from SecureStorage
            _ = LoadUserInfoAsync();
        }

        async Task LoadUserInfoAsync()
        {
            try
            {
                UserName = await SecureStorage.GetAsync("username") ?? "User";
                Email = await SecureStorage.GetAsync("email") ?? "user@example.com";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user info: {ex.Message}");
            }
        }


        // Load reviews của mình
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

        // Navigate to AddReviewPage
        [RelayCommand]
        async Task AddReviewAsync()
        {
            try
            {
                await Shell.Current.Navigation.PushAsync(new Pages.AddReviewPage());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation error: {ex.Message}");
            }            
        }

        // Navigate to AddReviewPage (Edit mode)
        [RelayCommand]
        async Task EditReviewAsync(Review review)
        {
            if (review == null) return;
            try
            {
                await Shell.Current.Navigation.PushAsync(new Pages.AddReviewPage(review));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation error: {ex.Message}");
            }
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
                MyReviews.Remove(review);
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

        // Navigate to AccountPage
        [RelayCommand]
        async Task GoToAccountAsync()
        {
            await Shell.Current.Navigation.PushAsync(new Pages.MajorPage.AccountPage());
        }
    }
}
