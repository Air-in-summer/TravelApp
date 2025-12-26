using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using TravelApplication.Models;
using TravelApplication.Services;
namespace TravelApplication.Pages.MajorPage;

public partial class SuggestViewPopup : Popup
{
    public ObservableCollection<Stop> SuggestedStops { get; } = new();
    public SuggestViewPopup()
	{
		InitializeComponent();
        // Lấy kích thước thiết bị (đơn vị là pixel)
        var displayInfo = DeviceDisplay.MainDisplayInfo;

        // Đổi từ pixel sang đơn vị thiết kế
        var screenWidth = displayInfo.Width / displayInfo.Density;
        var screenHeight = displayInfo.Height / displayInfo.Density;

        // Chiều cao 2/3 màn hình
        var height = screenHeight * 2 / 3;

        this.Size = new Size(screenWidth, height);
        this.CanBeDismissedByTappingOutsideOfPopup = true;
    }

    public SuggestViewPopup(List<Stop> suggestedStops) : this()
    {
        BindingContext = this;
        LoadSuggestStops(suggestedStops);
    }

    private async void LoadSuggestStops(List<Stop> suggestedStops)
    {
        SuggestedStops.Clear();
        foreach(var stop in suggestedStops)
        {
            SuggestedStops.Add(stop);
        }
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        bool confirm = await Shell.Current.DisplayAlert(
                            "Xác nhận",
                            "Bạn có chắc chắn muốn cập nhật lịch trình theo gợi ý?",
                            "Có", // nút xác nhận
                            "Không" // nút hủy
        );

        if (!confirm)
        {
            Close();
            return;
        }
        Close();
        WeakReferenceMessenger.Default.Send(new UpdateStopMessage(1));
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        bool confirm = await Shell.Current.DisplayAlert(
                            "Xác nhận",
                            "Bạn có chắc chắn muốn hủy bỏ cập nhật lịch trình theo gợi ý?",
                            "Có", // nút xác nhận
                            "Không" // nút hủy
        );

        if (!confirm)
        {
            Close();
            return;
        }
        Close();
        WeakReferenceMessenger.Default.Send(new CancelUpdateStopMessage(1));
    }
}