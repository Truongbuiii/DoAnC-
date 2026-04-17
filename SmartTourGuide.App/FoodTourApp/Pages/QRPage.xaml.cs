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

        // Sửa dòng Formats bằng cách thêm namespace đầy đủ vào trước
        BarcodeReader.Options = new ZXing.Net.Maui.BarcodeReaderOptions
        {
            // Thêm "ZXing.Net.Maui." vào trước BarcodeFormat
            Formats = ZXing.Net.Maui.BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Lang.Load();
        ApplyLanguage();

        try
        {
            // Xin quyền camera trước khi bật IsDetecting để tránh văng app
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlertAsync(Lang.Get("btn_ok"), "Vui lòng cấp quyền camera", "OK");
                return;
            }

            _isProcessing = false;
            StatusLabel.Text = Lang.Get("qr_waiting");
            await Task.Delay(500); // Đợi UI vẽ xong
            BarcodeReader.IsDetecting = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi camera: {ex.Message}");
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
        BarcodeReader.IsDetecting = false; // Tắt camera khi chuyển tab
    }

    private void OnBarcodeDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_isProcessing) return;
        _isProcessing = true;
        BarcodeReader.IsDetecting = false;

        var result = e.Results.FirstOrDefault();
        if (result == null) { _isProcessing = false; return; }

        string code = result.Value;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
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
                    _ = Task.Run(async () =>
                    {
                        await _dbService.LogActivityAsync(poiId, "ScanQR", lang);
                        var apiSync = new ApiSyncService(_dbService);
                        await apiSync.SyncLogsAsync();
                    });
                    var poi = await _dbService.GetPOIByIdAsync(poiId);
                    if (poi != null)
                    {
                        // 1. CHUYỂN TRANG NGAY LẬP TỨC
                        await Navigation.PushAsync(new PoiDetailPage(poi));

                        // 2. ĐỌC THUYẾT MINH TRỰC TIẾP TỪ DescriptionVi
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                string textToSpeak = poi.DescriptionVi ?? string.Empty;
                                string voiceLang = "vi-VN"; // Mặc định giọng Việt

                                if (string.IsNullOrWhiteSpace(textToSpeak)) return;

                                if (!lang.ToLower().StartsWith("vi"))
                                {
                                    var translator = new TranslationService();
                                    var shortCode = lang.Split('-')[0];
                                    var translated = await translator.TranslateAsync(poi.DescriptionVi, shortCode);

                                    if (!string.IsNullOrEmpty(translated))
                                    {
                                        textToSpeak = translated;
                                        voiceLang = lang; // Đổi giọng ngoại ngữ nếu dịch thành công
                                    }
                                }
#if ANDROID
                                var tts = new FoodTourApp.Platforms.Android.AndroidTtsService();
                                await tts.InitializeAsync();
                                tts.SetLanguage(voiceLang);
                                tts.Speak(textToSpeak);
#endif
                            }
                            catch { }
                        });
                        return;
                    }
                }
                await DisplayAlertAsync(Lang.Get("qr_not_found"), Lang.Get("qr_not_found_msg"), "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xử lý QR: {ex.Message}");
            }
            finally
            {
                _isProcessing = false;
                // Không bật lại IsDetecting ở đây để tránh bị quét lặp khi đang chuyển trang
            }
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
        string? code = await DisplayPromptAsync(Lang.Get("qr_manual"), "Nhập ID:", "OK", "Hủy", keyboard: Keyboard.Numeric);
        if (string.IsNullOrEmpty(code)) return;

        if (int.TryParse(code, out int poiId))
        {
            var poi = await _dbService.GetPOIByIdAsync(poiId);
            if (poi != null) await Navigation.PushAsync(new PoiDetailPage(poi));
        }
    }
}