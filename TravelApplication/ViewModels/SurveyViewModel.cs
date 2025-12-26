using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TravelApplication.Models;
using TravelApplication.Pages.MajorPage;
using TravelApplication.Services;
namespace TravelApplication.ViewModels
{
    //[QueryProperty(nameof(TripId), "TripId")]
    public partial class SurveyViewModel : BaseViewModel
    {
        private readonly TripService tripService;
        public ObservableCollection<string> Categories { get; set; } = new()
        {
            "accomodation", "commercial", "catering", "entertainment", "heritage",
            "leisure", "natural", "national_park", "religion", "camping", "beach"
        };
        
        [ObservableProperty]
        private ObservableCollection<string> selectedCategories = new();

        public int TripId { get; set; }

        public SurveyViewModel(int tripId)
        {
            tripService = ServiceHelper.GetService<TripService>();
            TripId = tripId;
        }

        
        public async Task<bool> SubmitPreferencesAsync()
        {
            if (SelectedCategories.Count != 3)
            {
                await Shell.Current.DisplayAlert("Lỗi", "Vui lòng chọn đúng 3 sở thích.", "OK");
                return false;
            }

            IsBusy = true;
            try
            {
                await tripService.AddPreferencesAsync(TripId, SelectedCategories.ToList());
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", ex.Message, "OK");
                return false;
            }
            
            IsBusy = false;
            await Shell.Current.DisplayAlert("Thành công", "Đã lưu sở thích!", "OK");
            return true;
        }

        [RelayCommand]
        private void ToggleCategory(string category)
        {
            if (SelectedCategories.Contains(category))
            {
                SelectedCategories.Remove(category);
            }
            else
            {
                if (SelectedCategories.Count >= 3)
                {
                    Shell.Current.DisplayAlert("Giới hạn", "Chỉ được chọn tối đa 3 mục.", "OK");
                    return;
                }
                SelectedCategories.Add(category);
            }
            
            // Force UI to refresh by reassigning the collection
            SelectedCategories = new ObservableCollection<string>(SelectedCategories);
        }
    }
}
