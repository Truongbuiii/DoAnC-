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

                // ========================================================
                // 1. XỬ LÝ QR TỔNG (MASTER QR) - Kích hoạt hành trình tự động
                // ========================================================
                if (code == "vinhkhanh://start-tour")
                {
                    var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (status == PermissionStatus.Granted)
                    {
                        // 1. Đánh dấu bật chế độ tự động
                        Preferences.Set("IsAutoMode", true);

                        // 2. Reset tất cả trạng thái "Đã nghe" về false để bắt đầu tour mới
                        // Lưu ý: Tài cần thêm hàm ResetAllHeardStatusAsync() vào DatabaseService.cs
                        await _dbService.ResetAllHeardStatusAsync();

                        await DisplayAlert(Lang.Get("tab_qr"), "Hành trình tự động bắt đầu! Hãy di chuyển để trải nghiệm.", "OK");

                        // 3. Chạy hàm theo dõi vị trí ngầm (Hàng đợi âm thanh)
                        _ = StartAutoGuideService();

                        // Chuyển sang trang Bản đồ
                        await Shell.Current.GoToAsync("//MapPage");
                    }
                    else
                    {
                        await DisplayAlert("Thông báo", "Vui lòng cấp quyền vị trí để sử dụng chế độ tự động", "OK");
                        BarcodeReader.IsDetecting = true;
                    }
                    _isProcessing = false;
                    return; // Thoát hàm ngay lập tức
                }

                // ========================================================
                // 2. XỬ LÝ QUÉT POI LẺ (Giữ nguyên logic của Tài)
                // ========================================================
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
                        await Navigation.PushAsync(new PoiDetailPage(poi));

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                string textToSpeak = poi.DescriptionVi ?? string.Empty;
                                string voiceLang = "vi-VN";

                                if (string.IsNullOrWhiteSpace(textToSpeak)) return;

                                if (!lang.ToLower().StartsWith("vi"))
                                {
                                    var translator = new TranslationService();
                                    var shortCode = lang.Split('-')[0];
                                    var translated = await translator.TranslateAsync(poi.DescriptionVi, shortCode);

                                    if (!string.IsNullOrEmpty(translated))
                                    {
                                        textToSpeak = translated;
                                        voiceLang = lang;
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
                await DisplayAlert(Lang.Get("qr_not_found"), Lang.Get("qr_not_found_msg"), "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xử lý QR: {ex.Message}");
            }
            finally
            {
                _isProcessing = false;
            }
        });
    }

    // Hàm bổ trợ để chạy ngầm dịch vụ hướng dẫn (Tài cần hiện thực chi tiết hàm này nhé)
    private async Task StartAutoGuideService()
    {
        // Logic: Lấy GPS -> Tính khoảng cách -> So sánh Priority -> Phát tiếng
        System.Diagnostics.Debug.WriteLine("=== ĐÃ KÍCH HOẠT DỊCH VỤ HÀNG ĐỢI ÂM THANH ===");
    }

    private void OnFlashToggle(object sender, EventArgs e)
    {
        _isFlashOn = !_isFlashOn;
        BarcodeReader.IsTorchOn = _isFlashOn;
        BtnFlash.Text = _isFlashOn ? "🔦 Off" : Lang.Get("qr_flash");
    }

    private async void OnManualEntry(object sender, EventArgs e)
{
    string? input = await DisplayPromptAsync(
        "Tìm địa điểm", 
        "Nhập tên quán:", 
        "Tìm kiếm", "Hủy",
        keyboard: Keyboard.Text);
        
    if (string.IsNullOrEmpty(input)) return;

    // Tìm POI theo tên (không phân biệt hoa thường)
    var allPois = await _dbService.GetPOIsAsync();
    var poi = allPois.FirstOrDefault(p => 
        p.Name.ToLower().Contains(input.ToLower()));

    if (poi != null)
        await Navigation.PushAsync(new PoiDetailPage(poi));
    else
        await DisplayAlertAsync("Không tìm thấy", 
            $"Không có địa điểm nào khớp với '{input}'", "OK");
    }
}