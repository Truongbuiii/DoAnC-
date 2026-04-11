using System.Collections.ObjectModel;
using FoodTourApp.Models;
using FoodTourApp.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Networking;

namespace FoodTourApp;

public partial class MainPage : ContentPage
{
    private readonly DatabaseService _dbService = new DatabaseService();
    private static bool _hasShownLanguagePicker = false;

    // --- CÁC BIẾN KÉT SẮT (CACHE RAM) ---
    private static bool _isInitialLoad = true;
    private static string _lastTranslatedLang = "";
    private bool _isDataLoaded = false;
    private List<POI> _cachedPois = new();
    private List<Itinerary> _cachedTours = new();

    private string _currentLanguage = "vi-VN";

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 1. Kiểm tra và chọn ngôn ngữ lần đầu (FIX LỖI LẶP 2 LẦN)
        if (!_hasShownLanguagePicker && !Preferences.ContainsKey("AppLanguage"))
        {
            _hasShownLanguagePicker = true; // 🔒 Khóa cửa ngay lập tức để chặn lần gọi thứ 2

            // Dùng Dispatcher để đợi giao diện App load xong hoàn toàn mới bật Popup
            Dispatcher.Dispatch(async () =>
            {
                await Task.Delay(300); // Trễ 0.3s để tạo hiệu ứng mượt mà

                string action = await DisplayActionSheetAsync(
                    "Chọn ngôn ngữ thuyết minh",
                    "Hủy", null,
                    "🇻🇳 Tiếng Việt",
                    "🇺🇸 English",
                    "🇨🇳 中文",
                    "🇰🇷 한국어",
                    "🇯🇵 日本語");

                string lang = action switch
                {
                    "🇺🇸 English" => "en-US",
                    "🇨🇳 中文" => "zh-CN",
                    "🇰🇷 한국어" => "ko-KR",
                    "🇯🇵 日本語" => "ja-JP",
                    _ => "vi-VN"
                };

                Preferences.Set("AppLanguage", lang);
                Lang.Set(lang);

                if (Shell.Current is AppShell appShell)
                    appShell.ApplyLanguage();

                // Sau khi người dùng chọn xong, ép hệ thống dịch giao diện ngay lập tức
                _currentLanguage = lang;
                _lastTranslatedLang = "";
                _ = TranslateDataAsync();
            });
        }
        else
        {
            // Lần sau mở App thì đọc thẳng từ Preferences
            _currentLanguage = Preferences.Get("AppLanguage", "vi-VN");
        }

        ApplyLanguage();

        // 2. LOGIC CACHE & CHỐNG CHỚP MÀN HÌNH
        if (!_isDataLoaded)
        {
            // LẦN ĐẦU MỞ APP
            _cachedPois = (await _dbService.GetPOIsAsync()).ToList();
            _cachedTours = (await _dbService.GetItinerariesAsync()).ToList();

            bool isVietnamese = _currentLanguage.StartsWith("vi", StringComparison.OrdinalIgnoreCase);

            // MẸO: Nếu là tiếng Anh, hiện dấu "..." trong lúc chờ dịch để giấu tiếng Việt đi
            foreach (var p in _cachedPois)
            {
                p.DisplayName = isVietnamese ? p.Name : "...";
                p.DisplayCategory = isVietnamese ? p.Category : "...";
            }
            foreach (var t in _cachedTours)
            {
                t.DisplayName = isVietnamese ? t.TourName : "...";
            }

            UpdateUI(); // Vẽ giao diện ngay lập tức

            if (!isVietnamese)
            {
                _ = TranslateDataAsync(); // Bắt đầu dịch ngầm
            }
            else
            {
                _lastTranslatedLang = _currentLanguage;
            }

            _isDataLoaded = true; // Chốt cờ: Lần sau quay lại không móc DB nữa!

            // 3. ĐỒNG BỘ MẠNG CHẠY NGẦM (Chỉ sync Data, không phá UI)
            if (_isInitialLoad)
            {
                await RunBackgroundSync();
                _isInitialLoad = false;
            }
        }
        else
        {
            // LẦN SAU QUAY LẠI: Dữ liệu đã có sẵn trong két sắt RAM
            if (_currentLanguage != _lastTranslatedLang)
            {
                bool isVietnamese = _currentLanguage.StartsWith("vi", StringComparison.OrdinalIgnoreCase);

                foreach (var p in _cachedPois)
                {
                    p.DisplayName = isVietnamese ? p.Name : "...";
                    p.DisplayCategory = isVietnamese ? p.Category : "...";
                }
                foreach (var t in _cachedTours)
                {
                    t.DisplayName = isVietnamese ? t.TourName : "...";
                }

                UpdateUI();

                if (!isVietnamese)
                {
                    _ = TranslateDataAsync();
                }
                else
                {
                    _lastTranslatedLang = _currentLanguage;
                }
            }
            else
            {
                // Ngôn ngữ KHÔNG ĐỔI -> Tải thẳng từ RAM lên màn hình (Mượt 100%, không chớp!)
                UpdateUI();
            }
        }
    }

    private void ApplyLanguage()
    {
        Lang.Load();
        LblWelcome.Text = Lang.Get("home_welcome");
        LblTitle.Text = Lang.Get("home_title");
        LblTop10.Text = Lang.Get("home_top10");
        LblExplore.Text = Lang.Get("home_explore");
        LblSubtitle.Text = Lang.Get("home_subtitle");
        LblTours.Text = Lang.Get("home_tours");
        LblFeatured.Text = Lang.Get("home_featured");
    }

    private void UpdateUI()
    {
        FeaturedPoisList.ItemsSource = null;
        ToursList.ItemsSource = null;
        FeaturedPoisList.ItemsSource = _cachedPois;
        ToursList.ItemsSource = _cachedTours;
    }

    private async Task TranslateDataAsync()
    {
        // Chặn dịch 2 lần
        if (_lastTranslatedLang == _currentLanguage) return;

        string targetLang = _currentLanguage; // Lưu lại ngôn ngữ đang muốn dịch

        try
        {
            var translator = new TranslationService();
            string shortCode = GetShortLangCode(targetLang);

            // Dịch POI (Nếu API lỗi thì fallback về lại tiếng Việt gốc thay vì để "...")
            foreach (var p in _cachedPois)
            {
                var dn = await translator.TranslateAsync(p.Name, shortCode);
                p.DisplayName = !string.IsNullOrEmpty(dn) ? dn : p.Name;

                var dc = await translator.TranslateAsync(p.Category, shortCode);
                p.DisplayCategory = !string.IsNullOrEmpty(dc) ? dc : p.Category;
            }

            foreach (var t in _cachedTours)
            {
                var dt = await translator.TranslateAsync(t.TourName, shortCode);
                t.DisplayName = !string.IsNullOrEmpty(dt) ? dt : t.TourName;
            }

            _lastTranslatedLang = targetLang; // Chốt cờ thành công
            MainThread.BeginInvokeOnMainThread(() => UpdateUI());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi dịch MainPage: {ex.Message}");
            // Bị lỗi thì fallback toàn bộ về tiếng Việt để không bị kẹt dấu "..."
            foreach (var p in _cachedPois) { p.DisplayName = p.Name; p.DisplayCategory = p.Category; }
            foreach (var t in _cachedTours) { t.DisplayName = t.TourName; }
            MainThread.BeginInvokeOnMainThread(() => UpdateUI());
        }
    }

    private async Task RunBackgroundSync()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet) return;

        var apiSync = new ApiSyncService(_dbService);
        _ = Task.Run(async () =>
        {
            await apiSync.SyncPoisAsync();
            await apiSync.SyncAudiosAsync();
            await apiSync.SyncToursAsync();
            await apiSync.SyncLogsAsync();

            // ĐÃ XÓA KHỐI LỆNH PHÁ HOẠI GIAO DIỆN Ở ĐÂY.
            // Dữ liệu mới đã được lưu vào SQLite. Lần sau mở App nó sẽ tự động update, 
            // không được phép ghi đè UI làm gián đoạn người dùng.
        });
    }

    private string GetShortLangCode(string culture)
    {
        if (string.IsNullOrEmpty(culture)) return "vi";
        return culture.Split('-')[0].ToLower();
    }

    private async void OnBannerTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//MapPage");

    private async void OnTourSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Itinerary tour) return;
        ((CollectionView)sender).SelectedItem = null;
        Preferences.Set("TourPoiIds", tour.PoiIdsRaw);
        Preferences.Set("TourName", tour.TourName);
        await Shell.Current.GoToAsync("//MapPage");
    }

    private async void OnPoiSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is POI selectedPoi)
        {
            await Navigation.PushAsync(new Pages.PoiDetailPage(selectedPoi));
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}