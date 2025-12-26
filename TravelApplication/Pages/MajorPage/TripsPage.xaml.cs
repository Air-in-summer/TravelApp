using TravelApplication.ViewModels;
using System.Windows.Input;
using System.Threading.Tasks;
using TravelApplication.Models;
namespace TravelApplication.Pages.MajorPage;

public partial class TripsPage : ContentPage
{
    private readonly TripsViewModel viewModel;
    public ICommand GoToTripDetailsCommand { get; }
    //public ICommand AddTripCommand { get; }
    public TripsPage()
    {
        InitializeComponent();
        viewModel = new TripsViewModel();
        BindingContext = viewModel;

        // Khởi tạo command
        GoToTripDetailsCommand = new Command<Trip>(OnGoToTripDetails);
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadTripsAsync();
    }

    private async void OnGoToTripDetails(Trip trip)
    {
        if (trip == null) return;
        await Navigation.PushAsync(new TripDetailPage(trip.Id));
    }

    private async void AddTripCommand(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddTripDestinationPage());
    }

}