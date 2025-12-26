using System.Collections.ObjectModel;
using System.Globalization;
using TravelApplication.ViewModels;
using System.Windows.Input;
namespace TravelApplication.Pages.MajorPage;

public partial class SurveyPage : ContentPage
{
	private readonly SurveyViewModel viewModel;
    
    int TripId { get; set; }
    public SurveyPage(int TripId)
	{
		InitializeComponent();
        this.TripId = TripId;
        viewModel = new SurveyViewModel(TripId);
        BindingContext = viewModel;
    }

	protected override async void OnAppearing()
	{
        base.OnAppearing();
    }

    private async void OnSubmitPreferencesClicked(object sender, EventArgs e)
    {
        bool success = await viewModel.SubmitPreferencesAsync();
        if (success)
        {
            await Navigation.PopToRootAsync();
        }
    }
}

// Converter for Background Color
public class CategoryBackgroundConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2) return Colors.White;
        
        var category = values[0] as string;
        var selected = values[1] as ObservableCollection<string>;

        bool isSelected = selected != null && selected.Contains(category);

        return isSelected
            ? Color.FromArgb("#2196F3")  // Blue when selected
            : Color.FromArgb("#FFFFFF"); // White when not selected
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

// Converter for Border Stroke
public class CategoryBorderConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2) return new SolidColorBrush(Colors.LightGray);
        
        var category = values[0] as string;
        var selected = values[1] as ObservableCollection<string>;

        bool isSelected = selected != null && selected.Contains(category);

        return isSelected
            ? new SolidColorBrush(Color.FromArgb("#2196F3"))  // Blue border when selected
            : new SolidColorBrush(Color.FromArgb("#E0E0E0")); // Light gray border when not selected
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

// Converter for Text Color
public class CategoryTextColorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2) return Colors.Black;
        
        var category = values[0] as string;
        var selected = values[1] as ObservableCollection<string>;

        bool isSelected = selected != null && selected.Contains(category);

        return isSelected
            ? Colors.White      // White text when selected
            : Colors.Black;     // Black text when not selected
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

// Converter for Checkmark visibility
public class CategoryCheckmarkConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2) return "○";
        
        var category = values[0] as string;
        var selected = values[1] as ObservableCollection<string>;

        bool isSelected = selected != null && selected.Contains(category);

        return isSelected ? "✓" : "○";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
