using Microsoft.Maui.Controls.Shapes;
using TravelApplication.Pages.ManagePage;
using TravelApplication.Services;

namespace TravelApplication.Pages.MajorPage;

public partial class AccountPage : ContentPage
{
	public AccountPage()
	{
		InitializeComponent();
	}

    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        // Create modern popup with styled entries
        var oldPasswordEntry = new Entry 
        { 
            Placeholder = "Old Password", 
            IsPassword = true,
            FontSize = 16
        };
        var newPasswordEntry = new Entry 
        { 
            Placeholder = "New Password", 
            IsPassword = true,
            FontSize = 16
        };
        var confirmPasswordEntry = new Entry 
        { 
            Placeholder = "Confirm New Password", 
            IsPassword = true,
            FontSize = 16
        };

        var popup = new ContentPage
        {
            BackgroundColor = Color.FromArgb("#F5F5F5"),
            Title = "Change Password",
            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Spacing = 20,
                    Padding = 20,
                    Children =
                    {
                        // Header
                        new Label 
                        { 
                            Text = "Change Password", 
                            FontSize = 24, 
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Color.FromArgb("#212121"),
                            Margin = new Thickness(0, 10, 0, 10)
                        },
                        
                        // Old Password Card
                        CreatePasswordCard("🔐 Old Password", oldPasswordEntry),
                        
                        // New Password Card
                        CreatePasswordCard("🔑 New Password", newPasswordEntry),
                        
                        // Confirm Password Card
                        CreatePasswordCard("✅ Confirm Password", confirmPasswordEntry),
                        
                        // Change Password Button
                        new Border
                        {
                            StrokeThickness = 0,
                            BackgroundColor = Color.FromArgb("#4CAF50"),
                            Padding = 0,
                            Margin = new Thickness(0, 10, 0, 0),
                            StrokeShape = new RoundRectangle { CornerRadius = 12 },
                            Shadow = new Shadow 
                            { 
                                Brush = Color.FromArgb("#4CAF50"), 
                                Opacity = 0.3f, 
                                Radius = 10, 
                                Offset = new Point(0, 4) 
                            },
                            GestureRecognizers =
                            {
                                new TapGestureRecognizer
                                {
                                    Command = new Command(async () =>
                                    {
                                        await ChangePasswordAsync(oldPasswordEntry.Text, newPasswordEntry.Text, confirmPasswordEntry.Text);
                                        await Navigation.PopAsync();
                                    })
                                }
                            },
                            Content = new Label
                            {
                                Text = "💾 Change Password",
                                FontSize = 16,
                                FontAttributes = FontAttributes.Bold,
                                TextColor = Colors.White,
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center,
                                Padding = new Thickness(0, 15)
                            }
                        },
                        
                        // Cancel Button
                        new Border
                        {
                            StrokeThickness = 1,
                            Stroke = Color.FromArgb("#BDBDBD"),
                            BackgroundColor = Colors.Transparent,
                            Padding = 0,
                            StrokeShape = new RoundRectangle { CornerRadius = 12 },
                            GestureRecognizers =
                            {
                                new TapGestureRecognizer
                                {
                                    Command = new Command(async () => await Navigation.PopAsync())
                                }
                            },
                            Content = new Label
                            {
                                Text = "Cancel",
                                FontSize = 16,
                                FontAttributes = FontAttributes.Bold,
                                TextColor = Color.FromArgb("#757575"),
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center,
                                Padding = new Thickness(0, 15)
                            }
                        }
                    }
                }
            }
        };

        await Navigation.PushAsync(popup);
    }

    private Border CreatePasswordCard(string label, Entry entry)
    {
        return new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Colors.White,
            Padding = 15,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Shadow = new Shadow 
            { 
                Brush = Colors.Black, 
                Opacity = 0.08f, 
                Radius = 8, 
                Offset = new Point(0, 2) 
            },
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    new Label
                    {
                        Text = label,
                        FontSize = 14,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#2196F3")
                    },
                    entry
                }
            }
        };
    }

    private async Task ChangePasswordAsync(string oldPassword, string newPassword, string confirmPassword)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            await DisplayAlert("Error", "All fields are required", "OK");
            return;
        }

        if (newPassword != confirmPassword)
        {
            await DisplayAlert("Error", "New passwords do not match", "OK");
            return;
        }

        if (newPassword.Length < 6)
        {
            await DisplayAlert("Error", "New password must be at least 6 characters", "OK");
            return;
        }

        try
        {
            var authService = ServiceHelper.GetService<AuthService>();
            var result = await authService.ChangePasswordAsync(oldPassword, newPassword);

            if (result)
            {
                await DisplayAlert("Success", "Password changed successfully", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Failed to change password. Please check your old password.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        SecureStorage.RemoveAll();
        // Chuyển về trang HomePage
        Application.Current.MainPage = new NavigationPage(new LoginPage());
    }
}