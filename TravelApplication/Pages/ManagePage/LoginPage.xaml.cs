using System.ComponentModel;
using TravelApplication.Services;
namespace TravelApplication.Pages.ManagePage;

public partial class LoginPage : ContentPage, INotifyPropertyChanged
{
    private readonly AuthService authService;
    public LoginPage()
    {
        InitializeComponent();
        
        authService = ServiceHelper.GetService<AuthService>();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // nhận dữ liệu từ các trường nhập và gọi dịch vụ đăng nhập
        var username = UsernameEntry?.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry.Text;
        var result = await authService.LoginAsync(username, password);

        if (!result.Success)
        {
            ErrorLabel.Text = result.ErrorMessage;
            ErrorLabel.IsVisible = true;
        }
        else
        {
            await DisplayAlert("Success", "Login successful!", "OK");
            Application.Current.MainPage = new AppShell();
        }

    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        // Navigate to register page
        await Navigation.PushAsync(new RegisterPage());
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ErrorLabel.IsVisible = false;
        UsernameEntry.Text = string.Empty;
        PasswordEntry.Text = string.Empty;
        NavigationPage.SetHasNavigationBar(this, false);
    }
}