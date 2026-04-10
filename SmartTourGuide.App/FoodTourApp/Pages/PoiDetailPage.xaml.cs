using FoodTourApp.Models;
using FoodTourApp.Services;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel; // Cần thiết cho ObservableCollection

#if ANDROID
using FoodTourApp.Platforms.Android;
#endif

namespace FoodTourApp.Pages;

public partial class PoiDetailPage : ContentPage
{
    private readonly POI _poi;
    private string _currentLanguage = "vi-VN";
    private readonly DatabaseService _dbService = new DatabaseService();

    // --- LỖI 1: CẦN KHAI BÁO BIẾN NÀY ĐỂ BINDING VÀO XAML ---
    public ObservableCollection<MenuItemModel> MenuItems { get; set; } = new();

#if ANDROID
    private AndroidTtsService? _androidTts;
#endif

    public PoiDetailPage(POI poi)
    {
        InitializeComponent();
        _poi = poi;
        BindingContext = _poi; // Gán để hiện Tên quán, Ảnh quán...

        WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                _currentLanguage = m.Value;
                Lang.Load();

                var freshPoi = await _dbService.GetPOIByIdAsync(_poi.PoiId);
                var displayPoi = freshPoi ?? _poi;
                BindingContext = displayPoi;

                // Always display Vietnamese first, then try to translate if needed
                DetailDescription.Text = displayPoi.DescriptionVi;
                if (!IsVietnamese(_currentLanguage))
                {
                    var translated = await GetDisplayDescriptionAsync(displayPoi);
                    if (!string.IsNullOrEmpty(translated))
                        DetailDescription.Text = translated;
                }

                ApplyLanguage();
                UpdateFavoriteButton();
                await LoadMenuData(); // Cập nhật lại món ăn nếu cần
            });
        });
    }

    // --- LỖI 2: HÀM NÀY CẦN ĐƯỢC GỌI TRONG ONAPPEARING ---
    private async Task LoadMenuData()
    {
        try
        {
            var items = await _dbService.GetMenuItemsByPoiIdAsync(_poi.PoiId);

            MainThread.BeginInvokeOnMainThread(() => {
                MenuItems.Clear();
                foreach (var item in items)
                {
                    MenuItems.Add(item);
                }

                // Gán nguồn dữ liệu cho CollectionView
                MenuCollectionView.ItemsSource = MenuItems;

                // Hiện/Ẩn tiêu đề "Món ngon phải thử"
                if (LblMenuTitle != null)
                    LblMenuTitle.IsVisible = MenuItems.Count > 0;
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== Lỗi load món ăn: {ex.Message}");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _currentLanguage = Preferences.Get("AppLanguage", "vi-VN");
        Lang.Load();

        // 1. Lấy dữ liệu mới nhất từ DB
        var poiFromDb = await _dbService.GetPOIByIdAsync(_poi.PoiId);
        var displayPoi = poiFromDb ?? _poi;

        BindingContext = displayPoi; // CẬP NHẬT LẠI ĐỂ HIỆN TÊN QUÁN

        // 2. HIỆN MÓN ĂN (QUAN TRỌNG: PHẢI GỌI Ở ĐÂY)
        await LoadMenuData();

        // 3. Hiện nội dung mô tả: luôn lấy DescriptionVi làm gốc
        DetailDescription.Text = displayPoi.DescriptionVi;

        // 4. Nếu ngôn ngữ hệ thống không phải tiếng Việt thì thực hiện dịch on-demand
        if (!IsVietnamese(_currentLanguage))
        {
            var translated = await GetDisplayDescriptionAsync(displayPoi);
            if (!string.IsNullOrEmpty(translated))
                DetailDescription.Text = translated;
        }

        ApplyLanguage();
        UpdateFavoriteButton();

#if ANDROID
        _androidTts = new AndroidTtsService();
        await _androidTts.InitializeAsync();
#endif
    }

    // Trả về chuỗi hiển thị (dịch nếu cần). Không ghi vào model.
    private async Task<string?> GetDisplayDescriptionAsync(POI poi)
    {
        if (poi == null || string.IsNullOrEmpty(poi.DescriptionVi)) return null;

        var shortCode = GetShortLangCode(_currentLanguage);
        if (string.IsNullOrEmpty(shortCode) || shortCode == "vi") return poi.DescriptionVi;

        try
        {
            TranslationLoader.IsVisible = true;
            TranslationLoader.IsRunning = true;

            var translator = new TranslationService();
            var translated = await translator.TranslateAsync(poi.DescriptionVi, shortCode);
            return string.IsNullOrEmpty(translated) ? poi.DescriptionVi : translated;
        }
        catch
        {
            return poi.DescriptionVi;
        }
        finally
        {
            TranslationLoader.IsRunning = false;
            TranslationLoader.IsVisible = false;
        }
    }

    private static bool IsVietnamese(string culture)
    {
        if (string.IsNullOrEmpty(culture)) return false;
        return culture.ToLower().StartsWith("vi");
    }

    private static string GetShortLangCode(string culture)
    {
        if (string.IsNullOrEmpty(culture)) return string.Empty;
        culture = culture.ToLower();
        if (culture.StartsWith("en")) return "en";
        if (culture.StartsWith("ja")) return "ja";
        if (culture.StartsWith("ko")) return "ko";
        if (culture.StartsWith("zh")) return "zh";
        return string.Empty;
    }

    // --- CÁC HÀM SỰ KIỆN GIỮ NGUYÊN ---
    private void ApplyLanguage()
    {
        LblIntroTitle.Text = Lang.Get("detail_intro");
        LblMenuTitle.Text = Lang.Get("detail_menu");
        BtnListen.Text = Lang.Get("detail_listen");
        BtnMap.Text = Lang.Get("detail_map");
        BtnNavigate.Text = Lang.Get("detail_navigate");
    }

    private void UpdateFavoriteButton()
    {
        if (_poi == null) return;
        BtnFavorite.Text = FavoritesPage.IsFavorite(_poi.PoiId)
            ? Lang.Get("detail_favorited")
            : Lang.Get("detail_favorite");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
#if ANDROID
        _androidTts?.Stop();
#endif
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();

    private void OnFavoriteClicked(object sender, EventArgs e)
    {
        if (_poi == null) return;
        if (FavoritesPage.IsFavorite(_poi.PoiId)) FavoritesPage.RemoveFavorite(_poi.PoiId);
        else FavoritesPage.AddFavorite(_poi.PoiId);
        UpdateFavoriteButton();
    }

    private async void OnSpeakClicked(object sender, EventArgs e)
    {
        var freshPoi = await _dbService.GetPOIByIdAsync(_poi.PoiId);
        var displayPoi = freshPoi ?? _poi;

        string text;
        if (IsVietnamese(_currentLanguage))
        {
            text = displayPoi.DescriptionVi;
        }
        else
        {
            text = await GetDisplayDescriptionAsync(displayPoi) ?? displayPoi.DescriptionVi;
        }
#if ANDROID
        _androidTts?.SetLanguage(_currentLanguage);
        _androidTts?.Speak(text);
#endif
    }

    private async void OnDirectClicked(object sender, EventArgs e)
    {
        try
        {
            var location = new Location(_poi.Latitude, _poi.Longitude);
            await Map.Default.OpenAsync(location, new MapLaunchOptions { Name = _poi.Name, NavigationMode = NavigationMode.Walking });
        }
        catch
        {
            await Launcher.OpenAsync($"google.navigation:q={_poi.Latitude},{_poi.Longitude}&mode=w");
        }
    }

    private async void OnViewOnMapClicked(object sender, EventArgs e)
    {
        Preferences.Set("HighlightPoiId", _poi.PoiId);
        await Shell.Current.GoToAsync("//MapPage");
    }
}