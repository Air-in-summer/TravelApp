using TravelApplication.Models;
using TravelApplication.ViewModels;

namespace TravelApplication.Pages.MajorPage;

public partial class AddStopPage : ContentPage
{
	int TripId { get; set; }
	Stop StopToEdit { get; set; }
	private readonly AddStopViewModel addStopViewModel;
	GeoapifyPlacesResult GeoapifyPlacesResult { get; set; }
    public AddStopPage(int tripId, GeoapifyPlacesResult result)
	{
		InitializeComponent();
		TripId = tripId;
		GeoapifyPlacesResult = result;
        addStopViewModel = new AddStopViewModel(tripId, result);

        BindingContext = addStopViewModel;
    }

	public AddStopPage(Stop stopToEdit)
	{
		InitializeComponent();
		StopToEdit = stopToEdit;
		addStopViewModel = new AddStopViewModel(StopToEdit);
		BindingContext = addStopViewModel;
    }
}