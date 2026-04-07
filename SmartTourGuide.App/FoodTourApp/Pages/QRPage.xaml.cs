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
        Lang.Load();
        ApplyLanguage();

        try
        {
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlertAsync(Lang.Get("btn_ok"),
                    "Vui lòng cấp quyền camera để quét mã QR", Lang.Get("btn_ok"));
                return;
            }

            _isProcessing = false;
            StatusLabel.Text = Lang.Get("qr_waiting");
            await Task.Delay(1000);
            BarcodeReader.IsDetecting = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Lỗi camera", ex.Message, Lang.Get("btn_ok"));
        }
    }

    private void ApplyLanguage()
    {
        Title = Lang.Get("tab_qr");
            LblHint.Text = Lang.Get("qr_hint");
        StatusLabel.Text = Lang.Get("qr_waiting");
        BtnFlash.Text = Lang.Get("qr_flash");
        BtnManual.Text = Lang.Get("qr_manual");
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
            StatusLabel.Text = $"✅ {code}";

            int poiId = -1;
            if (code.StartsWith("POI_"))
                int.TryParse(code.Replace("POI_", ""), out poiId);
            else
                int.TryParse(code, out poiId);

            if (poiId > 0)
            {
                var lang = Preferences.Get("AppLanguage", "vi-VN");
                _ = _dbService.LogActivityAsync(poiId, "ScanQR", lang);

                var poi = await _dbService.GetPOIByIdAsync(poiId);
                if (poi != null)
                {
                    // Phát TTS ngay lập tức
                    var ttsLang = Preferences.Get("AppLanguage", "vi-VN");
#if ANDROID
                    var tts = new FoodTourApp.Platforms.Android.AndroidTtsService();
                    await tts.InitializeAsync();
                    tts.SetLanguage(lang);
                    tts.Speak(poi.GetDescription(lang));
#endif

                    await Navigation.PushAsync(new PoiDetailPage(poi));
                    _isProcessing = false;
                    BarcodeReader.IsDetecting = true;
                    return;
                }
            }

            await DisplayAlertAsync(Lang.Get("qr_not_found"),
                $"{Lang.Get("qr_not_found_msg")} '{code}'", Lang.Get("btn_ok"));

            _isProcessing = false;
            BarcodeReader.IsDetecting = true;
            StatusLabel.Text = Lang.Get("qr_waiting");
        });
    }

    private void OnFlashToggle(object sender, EventArgs e)
    {
        _isFlashOn = !_isFlashOn;
        BarcodeReader.IsTorchOn = _isFlashOn;
        BtnFlash.Text = _isFlashOn ? "🔦 Off" : Lang.Get("qr_flash");
    }

    private async void OnManualEntry(object sender, EventArgs e)
    {
        string? code = await DisplayPromptAsync(
            Lang.Get("qr_manual"),
            "1-10:", Lang.Get("btn_ok"), Lang.Get("btn_cancel"),
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

        await DisplayAlertAsync(Lang.Get("qr_not_found"),
            Lang.Get("qr_not_found_msg"), Lang.Get("btn_ok"));
    }
}