using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TravelApplication.Models;
using TravelApplication.Services;
using TravelApplication.Pages.MajorPage;



namespace TravelApplication.ViewModels
{
    public partial class TripsViewModel : BaseViewModel
    {
        private readonly TripService tripService;
        public ObservableCollection<Trip> Trips { get; set; } = new ObservableCollection<Trip>();

        public TripsViewModel()
        {
            tripService = ServiceHelper.GetService<TripService>();
            Title = "My Trips ✈️";
        }

        [RelayCommand]
        public async Task LoadTripsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try // 👈 Add this
            {
                Trips.Clear();
                var trips = await tripService.GetTripsAsync();
                foreach (var trip in trips)
                {
                    Trips.Add(trip);
                }
            }
            catch (Exception ex) // 👈 Add this
            {
                // This will show the REAL error message
                string innerMessage = ex.InnerException?.Message ?? ex.Message;
                await Shell.Current.DisplayAlert("Connection Error", $"Failed to connect to the API. Please check your network and firewall settings.\n\nError: {innerMessage}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

       

        [RelayCommand]
        async Task DeleteTripAsync(Trip trip)
        {
            if (trip == null) return;

            bool confirmed = await Shell.Current.DisplayAlert("Confirm Delete", $"Are you sure you want to delete '{trip.Title}'?", "Yes", "No");
            if (confirmed)
            {
                IsBusy = true;
                try
                {
                    await tripService.DeleteTripAsync(trip.Id);
                    Trips.Remove(trip);
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error", $"Failed to delete trip: {ex.Message}", "OK");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }
    }
}
