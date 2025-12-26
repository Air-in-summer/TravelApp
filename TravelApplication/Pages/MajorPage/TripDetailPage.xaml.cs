
using System.Threading.Tasks;
using TravelApplication.ViewModels;


namespace TravelApplication.Pages.MajorPage;

public partial class TripDetailPage : ContentPage
{
	private readonly TripDetailViewModel viewModel;
	int TripId { get; set; }
	public TripDetailPage()
	{
		InitializeComponent();
		viewModel = new TripDetailViewModel(); 
        BindingContext = viewModel;
    }

	public TripDetailPage(int tripId)
	{
		InitializeComponent();
		TripId = tripId;
		viewModel = new TripDetailViewModel(TripId);
		BindingContext = viewModel;

	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await viewModel.LoadTripAsync();
    }

    private async void OnAddStopClicked(object sender, EventArgs e)
    {
		await Navigation.PushAsync(new AddStopDestinationPage(TripId));
    }

	private async void OnSuggestTripClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new SuggestPage(TripId));
    }

	private async void OnViewMapClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new MapPage(TripId));
    }

	private async void OnCheckWeatherClicked(object sender, EventArgs e)
	{
		if (viewModel.CurrentTrip == null) return;

		// Tính điểm giữa của bounding box
		double centerLat = (viewModel.CurrentTrip.MinLat + viewModel.CurrentTrip.MaxLat) / 2;
		double centerLon = (viewModel.CurrentTrip.MinLon + viewModel.CurrentTrip.MaxLon) / 2;

		var navigationParameter = new Dictionary<string, object>
		{
			{ "Latitude", centerLat },
			{ "Longitude", centerLon },
			{ "PlaceName", viewModel.CurrentTrip.DestinationName }
		};

		await Navigation.PushAsync(new WeatherPage(navigationParameter));
    }

	private async void OnYourPreferencesClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new PreferenceRecommendationsPage(TripId));
	}
}