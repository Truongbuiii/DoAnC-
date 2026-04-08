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
    private string _currentLanguage;
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
                if (freshPoi != null)
                {
                    BindingContext = freshPoi;
                    DetailDescription.Text = freshPoi.GetDescription(_currentLanguage);
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

        // 3. Hiện nội dung mô tả
        DetailDescription.Text = displayPoi.GetDescription(_currentLanguage);

        // 4. Kiểm tra dịch tự động
        if (IsTranslationMissing(displayPoi, _currentLanguage))
        {
            await HandleTranslationAsync(displayPoi);
        }

        ApplyLanguage();
        UpdateFavoriteButton();

#if ANDROID
        _androidTts = new AndroidTtsService();
        await _androidTts.InitializeAsync();
#endif
    }

    // Tách riêng logic dịch cho sạch code
    private async Task HandleTranslationAsync(POI poi)
    {
        try
        {
            TranslationLoader.IsVisible = true;
            TranslationLoader.IsRunning = true;

            var translator = new TranslationService();
            await Task.Run(async () => await translator.TranslatePoiAsync(poi));
            await _dbService.SavePOIAsync(poi);

            MainThread.BeginInvokeOnMainThread(() => {
                DetailDescription.Text = poi.GetDescription(_currentLanguage);
            });
        }
        finally
        {
            TranslationLoader.IsRunning = false;
            TranslationLoader.IsVisible = false;
        }
    }

    private bool IsTranslationMissing(POI p, string lang)
    {
        return lang switch
        {
            "vi-VN" => false,
            "en-US" => string.IsNullOrEmpty(p.DescriptionEn),
            "ja-JP" => string.IsNullOrEmpty(p.DescriptionJa),
            "ko-KR" => string.IsNullOrEmpty(p.DescriptionKo),
            "zh-CN" => string.IsNullOrEmpty(p.DescriptionZh),
            _ => true
        };
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
        string text = (freshPoi ?? _poi).GetDescription(_currentLanguage);
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