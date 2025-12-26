using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mapsui.Nts.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelApplication.Models;
using TravelApplication.Pages.MajorPage;
using TravelApplication.Services;
using TravelApplication.Models;

namespace TravelApplication.ViewModels
{
    // ViewModel cho việc thêm mới hoặc chỉnh sửa chuyến đi
    public partial class AddTripViewModel : BaseViewModel
    {
        private readonly TripService tripService;
        
        GeoapifyResult SelectedLocation;

        [ObservableProperty]
        Trip trip = new();

        [ObservableProperty]
        Trip tripToEdit = default!; // Fixed warning
        public bool IsEditMode => tripToEdit != null;

        Trip createdTrip;

        // Constructor cho chế độ thêm mới (nhận địa điểm từ Geoapify)
        public AddTripViewModel(GeoapifyResult result)
        {
            tripService = ServiceHelper.GetService<TripService>();
            SelectedLocation = result;
            Title = "Add Trip"; 
        }

        // Constructor cho chế độ chỉnh sửa (nhận trip hiện có)
        public AddTripViewModel(Trip trip)
        {
            tripService = ServiceHelper.GetService<TripService>();
            tripToEdit = trip;
            Title = "Edit Trip"; 
        }

        partial void OnTripToEditChanged(Trip value)
        {
            if (value != null)
            {
                Trip = new Trip
                {
                    Id = value.Id,
                    Title = value.Title,
                    Description = value.Description,
                    DestinationName = tripToEdit.DestinationName,
                    DestinationId = tripToEdit.DestinationId,
                    MinLon = tripToEdit.MinLon,
                    MinLat = tripToEdit.MinLat,
                    MaxLon = tripToEdit.MaxLon,
                    MaxLat = tripToEdit.MaxLat,
                    StartDate = value.StartDate.ToUniversalTime(),
                    EndDate = value.EndDate.ToUniversalTime(),
                    Budget = value.Budget,
                };
                Title = "Edit Trip";
            }
        }

        // Lưu chuyến đi (thêm mới hoặc cập nhật)
        // Trả về: ID của trip, hoặc -1 nếu lỗi
        public async Task<int> SaveTripAsync()
        {
            if (string.IsNullOrWhiteSpace(Trip.Title) || Trip.EndDate < Trip.StartDate)
            {                
                await Shell.Current.DisplayAlert("Validation Error", "Please provide a valid title and ensure the end date is after the start date.", "OK");
                return -1;
            }

            IsBusy = true;
            try
            {
                if (IsEditMode)
                {
                    // Chế độ chỉnh sửa
                    Trip.Id = tripToEdit.Id;
                    Trip.DestinationName = tripToEdit.DestinationName;
                    Trip.DestinationId = tripToEdit.DestinationId;
                    Trip.MinLon = tripToEdit.MinLon;
                    Trip.MinLat = tripToEdit.MinLat;
                    Trip.MaxLon = tripToEdit.MaxLon;
                    Trip.MaxLat = tripToEdit.MaxLat;
                    await tripService.UpdateTripAsync(Trip);
                    IsBusy = false;
                    return Trip.Id;
                }
                else
                {
                    // Chế độ thêm mới
                    Trip.DestinationName = SelectedLocation.DisplayName;
                    Trip.DestinationId = SelectedLocation.PlaceId;
                    Trip.MinLon = SelectedLocation.MinLon;
                    Trip.MinLat = SelectedLocation.MinLat;
                    Trip.MaxLon = SelectedLocation.MaxLon;
                    Trip.MaxLat = SelectedLocation.MaxLat;
                    Trip.StartDate = Trip.StartDate.ToUniversalTime();
                    Trip.EndDate = Trip.EndDate.ToUniversalTime();
                    createdTrip = await tripService.AddTripAsync(Trip);
                    IsBusy = false;
                    return createdTrip.Id;
                }
                
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to save trip: {ex.Message}", "OK");
                IsBusy = false;
                return -1;
            }
        }
    }
}
