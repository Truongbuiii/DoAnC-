namespace FoodTourApp.Pages;

public partial class ItineraryPage : ContentPage
{
    public ItineraryPage()
    {
        InitializeComponent();
    }

    // Bắt đầu Tour Ốc Huyền Thoại → chuyển sang tab Bản đồ
    private async void OnStartTour1(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlertAsync(
            "🦪 Tour Ốc Huyền Thoại",
            "Bắt đầu tour 90 phút qua 3 địa điểm ốc nổi tiếng nhất Vĩnh Khánh?",
            "Bắt đầu!", "Để sau");

        if (!confirm) return;

        // Chuyển sang tab Bản đồ
        await Shell.Current.GoToAsync("//MapPage");
    }

    // Bắt đầu Tour Ăn Vặt Xế Chiều
    private async void OnStartTour2(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlertAsync(
            "🍢 Ăn Vặt Xế Chiều",
            "Bắt đầu tour 60 phút qua 4 địa điểm ăn vặt đặc trưng?",
            "Bắt đầu!", "Để sau");

        if (!confirm) return;

        await Shell.Current.GoToAsync("//MapPage");
    }
}