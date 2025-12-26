using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelApplication.Models;
using TravelApplication.Services;

namespace TravelApplication.ViewModels
{
    public partial class AddStopViewModel : BaseViewModel
    {
        private readonly TripService tripService;
        private readonly StopService stopService;

        [ObservableProperty]
        Stop stop = new();

        Stop StopToEdit = default!;
        
        int TripId;
        GeoapifyPlacesResult GeoapifyPlacesResult { get; set; }

        // Thông tin trip để validate dates
        public DateTime TripStartDate { get; private set; }
        public DateTime TripEndDate { get; private set; }

        private bool IsEditMode => StopToEdit != null;

        public DateTime ArrivalDatePicker
        {
            get => Stop.ArrivalDate.Date;
            set
            {
                Stop.ArrivalDate = value.Date + Stop.ArrivalDate.TimeOfDay;
            }
        }

        public TimeSpan ArrivalTimePicker
        {
            get => Stop.ArrivalDate.TimeOfDay;
            set
            {
                Stop.ArrivalDate = Stop.ArrivalDate.Date + value;
            }
        }

        public DateTime DepartureDatePicker
        {
            get => Stop.DepartureDate.Date;
            set
            {
                Stop.DepartureDate = value.Date + Stop.DepartureDate.TimeOfDay;
            }
        }

        public TimeSpan DepartureTimePicker
        {
            get => Stop.DepartureDate.TimeOfDay;
            set
            {
                Stop.DepartureDate = Stop.DepartureDate.Date + value;
            }
        }


        public AddStopViewModel(int TripId, GeoapifyPlacesResult result)
        {
            this.TripId = TripId;
            GeoapifyPlacesResult = result;
            tripService = ServiceHelper.GetService<TripService>();
            stopService = ServiceHelper.GetService<StopService>();
            Title = "Add New Stop";
            
            // Load trip dates để validate
            _ = LoadTripDatesAsync();
        }
        
        public AddStopViewModel(Stop stopToEdit)
        {
            tripService = ServiceHelper.GetService<TripService>();
            stopService = ServiceHelper.GetService<StopService>();
            StopToEdit = stopToEdit;
            TripId = stopToEdit.TripId;
            Title = "Edit Stop";
            
            // Load trip dates để validate
            _ = LoadTripDatesAsync();
        }  

        // Load thông tin trip để lấy StartDate và EndDate
        private async Task LoadTripDatesAsync()
        {
            try
            {
                var trip = await tripService.GetTripAsync(TripId);
                if (trip != null)
                {
                    TripStartDate = trip.StartDate;
                    TripEndDate = trip.EndDate;
                    
                    // Fix: If same-day trip, set end time to 23:59:59 to allow stops
                    if (TripStartDate.Date == TripEndDate.Date)
                    {
                        TripEndDate = TripEndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    }
                }
            }
            catch (Exception ex)
            {
                // Nếu không load được trip, set default values
                TripStartDate = DateTime.MinValue;
                TripEndDate = DateTime.MaxValue;
            }
        }

        [RelayCommand]
        async Task SaveStopAsync()
        {    
            // Validate: ArrivalDate phải trước DepartureDate
            if (Stop.ArrivalDate > Stop.DepartureDate)
            {
                await Shell.Current.DisplayAlert("Invalid Dates", "Arrival date must be before departure date.", "OK");
                return;
            }
            
            // Validate: Stop dates phải nằm trong khoảng Trip dates
            if (Stop.ArrivalDate < TripStartDate || Stop.DepartureDate > TripEndDate)
            {
                await Shell.Current.DisplayAlert(
                    "Invalid Dates", 
                    $"Stop dates must be within trip dates ({TripStartDate:dd/MM/yyyy} - {TripEndDate:dd/MM/yyyy}).", 
                    "OK");
                return;
            }
            
            IsBusy = true;
            try
            {
                if (IsEditMode)
                {                    
                    Stop.Id = StopToEdit.Id;
                    Stop.Location = StopToEdit.Location;
                    Stop.LocationId = StopToEdit.LocationId;
                    Stop.Longitude = StopToEdit.Longitude;
                    Stop.Latitude = StopToEdit.Latitude;
                    Stop.Category = StopToEdit.Category;
                    Stop.Address = StopToEdit.Address;
                    Stop.TripId = StopToEdit.TripId;
                    await stopService.UpdateStopAsync(Stop);
                }
                else
                {
                    Stop.TripId = this.TripId;
                    Stop.Location = GeoapifyPlacesResult.DisplayName;
                    Stop.LocationId = GeoapifyPlacesResult.LocationId;
                    Stop.Longitude = GeoapifyPlacesResult.Longitude;
                    Stop.Latitude = GeoapifyPlacesResult.Latitude;
                    Stop.Category = GeoapifyPlacesResult.Category;
                    Stop.Address = GeoapifyPlacesResult.Address;
                    Stop.ArrivalDate = Stop.ArrivalDate.ToUniversalTime();
                    Stop.DepartureDate = Stop.DepartureDate.ToUniversalTime();
                    await stopService.AddStopAsync(Stop);
                }
                
            }
            catch (HttpRequestException httpEx) // This is the new, more specific catch block
            {
                // This will display the DETAILED error message from the API
                await Shell.Current.DisplayAlert("API Error", httpEx.Message, "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
                await Shell.Current.Navigation.PopAsync();
            }
        }

    }
}
