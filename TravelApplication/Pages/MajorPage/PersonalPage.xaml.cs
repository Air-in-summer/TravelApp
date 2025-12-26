namespace TravelApplication.Pages.MajorPage;

public partial class PersonalPage : ContentPage
{
	public PersonalPage()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Load reviews khi page xuất hiện
        if (BindingContext is ViewModels.PersonalViewModel viewModel)
        {
            viewModel.LoadMyReviewsCommand.Execute(null);
        }
    }
}
