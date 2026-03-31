using FoodTourApp.Services;
using ZXing;
using ZXing.Net.Maui;

namespace FoodTourApp.Pages;

public partial class QRPage : ContentPage
{
    private readonly DatabaseService _dbService;
    private bool _isFlashOn = false;
    private bool _isProcessing = false;

    public QRPage()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            // Xin quyền camera
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlertAsync("Cần quyền camera",
                    "Vui lòng cấp quyền camera để quét mã QR", "OK");
                return;
            }

            _isProcessing = false;
            StatusLabel.Text = "📷 Đang chờ quét mã QR...";

            await Task.Delay(1000);
            BarcodeReader.IsDetecting = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Lỗi camera", ex.Message, "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        BarcodeReader.IsDetecting = false;
    }

    private async void OnBarcodeDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_isProcessing) return;
        _isProcessing = true;

        BarcodeReader.IsDetecting = false;

        var result = e.Results.FirstOrDefault();
        if (result == null) { _isProcessing = false; return; }

        string code = result.Value;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            StatusLabel.Text = $"✅ Đã quét: {code}";

            // Parse poiId TRƯỚC
            int poiId = -1;
            if (code.StartsWith("POI_"))
                int.TryParse(code.Replace("POI_", ""), out poiId);
            else
                int.TryParse(code, out poiId);

            // Ghi log ScanQR SAU KHI có poiId
            if (poiId > 0)
            {
                var lang = Preferences.Get("AppLanguage", "vi-VN");
                _ = _dbService.LogActivityAsync(poiId, "ScanQR", lang);

                var poi = await _dbService.GetPOIByIdAsync(poiId);
                if (poi != null)
                {
                    await Navigation.PushAsync(new PoiDetailPage(poi));
                    _isProcessing = false;
                    BarcodeReader.IsDetecting = true;
                    return;
                }
            }

            await DisplayAlertAsync("Không tìm thấy",
                $"Mã QR '{code}' không khớp với địa điểm nào!", "OK");

            _isProcessing = false;
            BarcodeReader.IsDetecting = true;
            StatusLabel.Text = "📷 Đang chờ quét mã QR...";
        });
    }

    private void OnFlashToggle(object sender, EventArgs e)
    {
        _isFlashOn = !_isFlashOn;
        BarcodeReader.IsTorchOn = _isFlashOn;
        ((Button)sender).Text = _isFlashOn ? "🔦 Tắt đèn" : "🔦 Đèn flash";
    }

    private async void OnManualEntry(object sender, EventArgs e)
    {
        string? code = await DisplayPromptAsync(
            "Nhập mã QR",
            "Nhập số POI (1-10):",
            "OK", "Hủy",
            keyboard: Keyboard.Numeric);

        if (string.IsNullOrEmpty(code)) return;

        if (int.TryParse(code, out int poiId))
        {
            var poi = await _dbService.GetPOIByIdAsync(poiId);
            if (poi != null)
            {
                await Navigation.PushAsync(new PoiDetailPage(poi));
                return;
            }
        }

        await DisplayAlertAsync("Lỗi", "Không tìm thấy địa điểm!", "OK");
    }
}