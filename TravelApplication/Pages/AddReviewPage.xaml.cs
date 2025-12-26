using TravelApplication.Models;
using TravelApplication.ViewModels;

namespace TravelApplication.Pages;

public partial class AddReviewPage : ContentPage
{
	public AddReviewPage()
	{
		InitializeComponent();
		BindingContext = new AddReviewViewModel();
	}

	// Constructor cho Edit mode
	public AddReviewPage(Review review)
	{
		InitializeComponent();
		BindingContext = new AddReviewViewModel(review);
	}
}
