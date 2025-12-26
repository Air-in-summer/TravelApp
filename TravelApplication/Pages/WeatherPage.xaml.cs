using TravelApplication.ViewModels;

namespace TravelApplication.Pages;

public partial class WeatherPage : ContentPage
{
	public WeatherPage()
	{
		InitializeComponent();
		BindingContext = new WeatherViewModel();
	}

	public WeatherPage(Dictionary<string, object> navigationParameter)
	{
		InitializeComponent();
		
		// Extract parameters
		double lat = (double)navigationParameter["Latitude"];
		double lon = (double)navigationParameter["Longitude"];
		string name = (string)navigationParameter["PlaceName"];
		
		// Create ViewModel với constructor parameters
		BindingContext = new WeatherViewModel(lat, lon, name);
	}
}
