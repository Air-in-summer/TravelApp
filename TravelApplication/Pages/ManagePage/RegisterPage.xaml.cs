using TravelApplication.Services;


namespace TravelApplication.Pages.ManagePage;

public partial class RegisterPage : ContentPage
{
    private readonly AuthService authService;
    public RegisterPage()
	{
		InitializeComponent();
        authService = ServiceHelper.GetService<AuthService>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        // nhận dữ liệu từ các trường nhập
        var username = UsernameEntry?.Text?.Trim() ?? string.Empty; 
        var email = EmailEntry?.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry?.Text ?? string.Empty;
        var confirmPassword = ConfirmPasswordEntry?.Text ?? string.Empty;

        var result = await authService.RegisterAsync(username, email, password, confirmPassword);


        if (!result.Success)
        {
            ErrorLabel.Text = result.ErrorMessage;
            ErrorLabel.IsVisible = true;
        }
        else
        {
            await DisplayAlert("Success", "Registration successful!", "OK");
            //trở về trang loginpage
            await Navigation.PopAsync();
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {       
        await Navigation.PopAsync();
    }
}