using CommunityToolkit.Maui.Views;
using TravelApplication.Models;
using TravelApplication.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using TravelApplication.Services;
namespace TravelApplication.Pages.MajorPage;

public partial class SuggestPage : ContentPage
{
	int TripId { get; set; }
	private readonly SuggestViewModel viewModel;

    private List<Stop> suggestedStops = new();
    public SuggestPage(int tripId)
	{
		InitializeComponent();
		viewModel = new SuggestViewModel(tripId);
		BindingContext = viewModel;
        TripId = tripId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadTripAsync();
        WeakReferenceMessenger.Default.Register<UpdateStopMessage>(this, async (r, m) =>
        {
            foreach (var stop in suggestedStops)
            {
                await viewModel.UpdateStopAsync(stop);
            }

            await DisplayAlert("Thành công", "Lịch trình đã được cập nhật!", "OK");
            await Navigation.PopAsync();
        });

        WeakReferenceMessenger.Default.Register<CancelUpdateStopMessage>(this, async (r, m) =>
        {
            await Navigation.PopAsync();
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.Unregister<UpdateStopMessage>(this);
        WeakReferenceMessenger.Default.Unregister<CancelUpdateStopMessage>(this);

    }

    private async void OnSuggestClicked(object sender, EventArgs e)
    {
        //sự kiện gợi ý lịch trình
        var selectedStops = viewModel.GetSelectedStops();
        var fixedStops = viewModel.GetUnselectedStops();
        var useBudget = viewModel.UseBudgetConstraint;
        var totalBudget = viewModel.CurrentTrip?.Budget ?? 0;
        var totalCost = selectedStops.Concat(fixedStops).Sum(s => s.EstimatedCost);
        var totalSelectedCost = selectedStops.Sum(s => s.EstimatedCost);
        var totalFixedCost = fixedStops.Sum(s => s.EstimatedCost);
        
        // Nếu không dùng budget → bỏ qua kiểm tra
        if (!useBudget)
        {
            suggestedStops = await viewModel.ProceedWithSuggestion(selectedStops, fixedStops, viewModel.CurrentTrip, useBudget, 0);
            LoadSuggest(suggestedStops);
            return;
        }

        // 🔹 Trường hợp 1: Chi phí cố định đã vượt ngân sách
        if (totalFixedCost > totalBudget)
        {
            var result = await DisplayAlert(
                "Vượt ngân sách",
                $"Chi phí các điểm cố định ({totalFixedCost:C}) đã vượt ngân sách chuyến đi ({totalBudget:C}).\n\nBạn có muốn tiếp tục gợi ý?",
                "Tiếp tục", "Hủy"
            );
            if (!result) return;

            // ➕ Sau khi chọn "Tiếp tục", cho 2 lựa chọn:
            var choice = await DisplayActionSheet(
                "Lựa chọn cách xử lý",
                "Hủy",
                null,
                "Gợi ý mà không áp dụng giới hạn ngân sách",
                "Nhập ngân sách mới"
            );

            if (choice == "Gợi ý mà không áp dụng giới hạn ngân sách")
            {
                useBudget = false;
                suggestedStops = await viewModel.ProceedWithSuggestion(selectedStops, fixedStops, viewModel.CurrentTrip, useBudget, 0);
                LoadSuggest(suggestedStops);
            }
            else if (choice == "Nhập ngân sách mới")
            {
                var newBudget = await HandleNewBudgetInput(totalFixedCost);
                if (newBudget == -1)
                {
                    // Người dùng đã hủy → dừng
                    return;
                }
                suggestedStops = await viewModel.ProceedWithSuggestion(selectedStops, fixedStops, viewModel.CurrentTrip, useBudget, newBudget - totalFixedCost);
                LoadSuggest(suggestedStops);
            }
            return;
        }

        // 🔹 Trường hợp 2: Chi phí flexible vượt phần ngân sách còn lại
        if (totalSelectedCost > totalBudget - totalFixedCost)
        {
            var result = await DisplayAlert(
                "Vượt ngân sách",
                $"Tổng chi phí ({totalCost:C}) vượt ngân sách ({totalBudget:C}).\n\nBạn có muốn tiếp tục gợi ý?",
                "Tiếp tục", "Hủy"
            );
            if (!result) return;

            // ➕ 3 lựa chọn sau khi chọn "Tiếp tục"
            var choice = await DisplayActionSheet(
                "Lựa chọn cách xử lý",
                "Hủy",
                null,
                "Gợi ý nhưng giữ nguyên ngân sách",
                "Nhập ngân sách mới",
                "Bỏ qua giới hạn ngân sách"
            );

            if (choice == "Gợi ý nhưng giữ nguyên ngân sách")
            {              
                var remainingBudget = totalBudget - totalFixedCost;
                suggestedStops = await viewModel.ProceedWithSuggestion(selectedStops, fixedStops, viewModel.CurrentTrip, useBudget, remainingBudget);
                LoadSuggest(suggestedStops);
            }
            else if (choice == "Nhập ngân sách mới")
            {
                var newBudget = await HandleNewBudgetInput(totalFixedCost);
                if (newBudget == -1)
                {                    
                    return;
                }
                suggestedStops = await viewModel.ProceedWithSuggestion(selectedStops, fixedStops, viewModel.CurrentTrip, useBudget, newBudget - totalFixedCost);
                LoadSuggest(suggestedStops);
            }
            else if (choice == "Bỏ qua giới hạn ngân sách")
            {
                useBudget = false;
                suggestedStops = await viewModel.ProceedWithSuggestion(selectedStops, fixedStops, viewModel.CurrentTrip, useBudget, 0);
                LoadSuggest(suggestedStops);
            }
            return;
        }
        else
        {
            useBudget = false;
            suggestedStops = await viewModel.ProceedWithSuggestion(selectedStops, fixedStops, viewModel.CurrentTrip, useBudget, 0);
            LoadSuggest(suggestedStops);
            return;
        }
    }

    private async Task<decimal> HandleNewBudgetInput(decimal totalFixedCost)
    {
        string placeholder = (totalFixedCost + 1).ToString("F0"); // gợi ý > fixed cost

        while (true)
        {
            var newBudgetStr = await DisplayPromptAsync(
                "Ngân sách mới",
                $"Nhập mức ngân sách mới (phải > {totalFixedCost:C}):",
                "Xác nhận", "Hủy",
                placeholder: placeholder,
                maxLength: 12,
                keyboard: Keyboard.Numeric
            );

            // Người dùng hủy
            if (string.IsNullOrEmpty(newBudgetStr))
            {
                return -1;
            }

            // Kiểm tra định dạng và giá trị
            if (decimal.TryParse(newBudgetStr, out var newBudget) && newBudget > totalFixedCost)
            {
                return newBudget;
            }

            // Nhập sai → báo lỗi và lặp lại
            await DisplayAlert("Lỗi", $"Ngân sách phải là số và lớn hơn {totalFixedCost:C}.", "OK");
            // tiếp tục vòng lặp
        }
    }

    private async void LoadSuggest(List<Stop> suggestedStops)
    {
        //gọi popup hiện kết quả gợi ý
        var bottomSheet = new SuggestViewPopup(suggestedStops);
        this.ShowPopup(bottomSheet);
    }

    
}