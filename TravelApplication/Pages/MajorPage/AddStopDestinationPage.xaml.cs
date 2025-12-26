using System.Windows.Input;
using Microsoft.Extensions.Configuration;
using TravelApplication.Models;
using TravelApplication.ViewModels;
namespace TravelApplication.Pages.MajorPage;

public partial class AddStopDestinationPage : ContentPage
{
	private readonly AddStopDestinationViewModel viewModel;
	public ICommand GoToAddStopCommand { get; }
	int TripId { get; set; }
    public AddStopDestinationPage(int tripId)
	{
		InitializeComponent();
		TripId = tripId;
        var configuration = ServiceHelper.GetService<IConfiguration>();
		viewModel = new AddStopDestinationViewModel(configuration, tripId);
		BindingContext = viewModel;
		GoToAddStopCommand = new Command<GeoapifyPlacesResult>(OnGoToAddStop);
    }

	protected override void OnAppearing()
	{
		base.OnAppearing();
	}

	private async void OnGoToAddStop(GeoapifyPlacesResult result)
	{
		if (result == null) return;
		await Navigation.PushAsync(new AddStopPage(TripId, result));
    }
}