using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TravelApplication.Models;
using TravelApplication.Services;
using TravelApplication.Pages.MajorPage;
#if ANDROID
using Android.Content;
#endif
namespace TravelApplication.ViewModels
{
    //[QueryProperty(nameof(TripId), "TripId")]
    public partial class TripDetailViewModel : BaseViewModel
    {
        private readonly TripService tripService;
        private readonly StopService stopService;
        private readonly PdfService pdfService;

        
        int TripId;

        [ObservableProperty]
        Trip currentTrip = default!;
        public ObservableCollection<Stop> Stops { get; } = new();

        [ObservableProperty]
        decimal totalCost;

        [ObservableProperty]
        bool isOverBudget;

        
        public TripDetailViewModel()
        {

        }
        public TripDetailViewModel(int TripId) 
        {
            Title = "Trip Details";
            tripService = ServiceHelper.GetService<TripService>();
            stopService = ServiceHelper.GetService<StopService>();
            pdfService = ServiceHelper.GetService<PdfService>();
            this.TripId = TripId;
        }

        

        public async Task LoadTripAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                CurrentTrip = await tripService.GetTripAsync(TripId);
                Stops.Clear();
                if (CurrentTrip?.Stops != null)
                {
                    foreach (var stop in CurrentTrip.Stops.OrderBy(s => s.ArrivalDate))
                    {
                        Stops.Add(stop);
                    }
                }
                Title = CurrentTrip?.Title ?? "Trip Details";
                CalculateCost();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load trip details: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void CalculateCost()
        {
            if (CurrentTrip == null) return;
            TotalCost = Stops.Sum(s => s.EstimatedCost);
            IsOverBudget = CurrentTrip.Budget > 0 && TotalCost > CurrentTrip.Budget;
        }

        [RelayCommand]
        async Task EditTripAsync()
        {
            if (CurrentTrip == null) return;
            
            // Điều hướng đến AddTripPage với CurrentTrip để chỉnh sửa
            await Shell.Current.Navigation.PushAsync(new AddTripPage(CurrentTrip));
        }

        
        [RelayCommand]
        async Task EditStopAsync(Stop stop)
        {
            if (stop == null) return;
            await Shell.Current.Navigation.PushAsync(new AddStopPage(stop));
            return;
        }

        [RelayCommand]
        async Task DeleteStopAsync(Stop stop)
        {
            if (stop == null) return;

            bool confirmed = await Shell.Current.DisplayAlert("Confirm Delete", $"Delete stop at '{stop.Location}'?", "Yes", "No");
            if (confirmed)
            {
                IsBusy = true;
                try
                {
                    await stopService.DeleteStopAsync(stop.Id);
                    Stops.Remove(stop);
                    CalculateCost();
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error", $"Failed to delete stop: {ex.Message}", "OK");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        [RelayCommand]
        async Task ExportToPdfAsync()
        {           
            string uriString = await pdfService.CreatePdfAsync(TripId);

            
            #if ANDROID
                var uri = Android.Net.Uri.Parse(uriString);
    
                var intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(uri, "application/pdf");
                intent.AddFlags(ActivityFlags.NewTask);
                intent.AddFlags(ActivityFlags.GrantReadUriPermission);

                Android.App.Application.Context.StartActivity(intent);
            #else
                // Windows/macOS/iOS
                await Launcher.OpenAsync(uriString);
            #endif
        }

        [RelayCommand]
        async Task ShareTripAsync()
        {
            
            return;
        }

        
    }
}
