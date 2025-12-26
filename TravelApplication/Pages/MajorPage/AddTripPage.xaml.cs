using TravelApplication.Models;
using TravelApplication.ViewModels;
namespace TravelApplication.Pages.MajorPage;


public partial class AddTripPage : ContentPage
{
    private readonly AddTripViewModel viewModel;
    public AddTripPage(GeoapifyResult result)
	{
		InitializeComponent();
        viewModel = new AddTripViewModel(result);
        BindingContext = viewModel;
        
    }

    public AddTripPage(Trip trip) 
    {
        InitializeComponent();
        viewModel = new AddTripViewModel(trip);
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();       
    }

    private async void OnSaveTripButtonClicked(object sender, EventArgs e)
    {
        int tripId = await viewModel.SaveTripAsync();
        if(tripId == -1)
            return;
        if(!viewModel.IsEditMode)
        {
            await Navigation.PushAsync(new SurveyPage(tripId));
        }
        else
        {
            await Navigation.PopAsync();
        }
        
    }
}