namespace FoodTourApp.Pages;

public partial class QRPage : ContentPage
{
    public QRPage()
    {
        InitializeComponent();
    }

    private async void OnFlashToggle(object sender, EventArgs e)
        => await DisplayAlertAsync("Thông báo", "Tính năng đèn flash sẽ được thêm sau.", "OK");

    private async void OnManualEntry(object sender, EventArgs e)
    {
        string code = await DisplayPromptAsync(
            "Nhập mã QR",
            "Nhập mã POI (1-10):",
            "OK", "Hủy",
            keyboard: Keyboard.Numeric);

        if (string.IsNullOrEmpty(code)) return;
        await DisplayAlertAsync("Mã QR", $"Đã nhận mã: {code}", "OK");
    }
}