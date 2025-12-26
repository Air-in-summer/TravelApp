namespace TravelApplication.Pages.MajorPage;

public partial class ReviewPage : ContentPage
{
	public ReviewPage()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Load reviews khi page xuất hiện
        if (BindingContext is ViewModels.ReviewViewModel viewModel)
        {
            viewModel.LoadAllReviewsCommand.Execute(null);
        }
    }
}