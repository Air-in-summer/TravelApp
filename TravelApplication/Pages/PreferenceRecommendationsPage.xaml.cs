using TravelApplication.ViewModels;

namespace TravelApplication.Pages;

public partial class PreferenceRecommendationsPage : ContentPage
{
	public PreferenceRecommendationsPage(int tripId)
	{
		InitializeComponent();
		BindingContext = new PreferenceRecommendationsViewModel(tripId);
	}
}
