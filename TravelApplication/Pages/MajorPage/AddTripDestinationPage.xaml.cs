using System.Windows.Input;
using Microsoft.Extensions.Configuration;
using TravelApplication.Models;
using TravelApplication.ViewModels;
namespace TravelApplication.Pages.MajorPage;

public partial class AddTripDestinationPage : ContentPage
{
	private readonly AddTripDestinationViewModel viewModel;

    public ICommand GoToAddTripCommand { get; }
    public AddTripDestinationPage()
	{
		InitializeComponent();
        var configuration = ServiceHelper.GetService<IConfiguration>();
        viewModel = new AddTripDestinationViewModel(configuration);
        BindingContext = viewModel;

        GoToAddTripCommand = new Command<GeoapifyResult>(OnGoToAddTrip);
    }

	protected override void OnAppearing()
	{
		base.OnAppearing();
    }

    private async void OnGoToAddTrip(GeoapifyResult result)
    {
        if (result == null) return;
        await Navigation.PushAsync(new AddTripPage(result));

    }


}