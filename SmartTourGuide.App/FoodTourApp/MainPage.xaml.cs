using System.Collections.ObjectModel;
using FoodTourApp.Models;
using FoodTourApp.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;

namespace FoodTourApp;

public partial class MainPage : ContentPage
{
    private readonly DatabaseService _dbService = new DatabaseService();
    private static bool _hasShownLanguagePicker = false;
    private string _currentLanguage = "vi-VN";

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 1. Kiểm tra và chọn ngôn ngữ lần đầu
        if (!_hasShownLanguagePicker && !Preferences.ContainsKey("AppLanguage"))
        {
            _hasShownLanguagePicker = true;
            await Task.Delay(500);

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
        }

        _currentLanguage = Preferences.Get("AppLanguage", "vi-VN");
        ApplyLanguage();
        await LoadDashboard();
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

    private async Task LoadDashboard()
    {
        var apiSync = new ApiSyncService(_dbService);
        _ = Task.Run(async () =>
        {
            await apiSync.SyncPoisAsync();
            await apiSync.SyncAudiosAsync(); // Đồng bộ kịch bản AI
            await apiSync.SyncToursAsync();
            await apiSync.SyncLogsAsync();

            MainThread.BeginInvokeOnMainThread(async () =>
                await LoadData());
        });

        await LoadData();
    }

    private async Task LoadData()
    {
        var allPois = (await _dbService.GetPOIsAsync()).ToList();
        var allTours = (await _dbService.GetItinerariesAsync()).ToList();

        // Gán tên mặc định
        foreach (var p in allPois) { p.DisplayName = p.Name; p.DisplayCategory = p.Category; }
        foreach (var t in allTours) { t.DisplayName = t.TourName; }

        FeaturedPoisList.ItemsSource = new ObservableCollection<POI>(allPois);
        ToursList.ItemsSource = new ObservableCollection<Itinerary>(allTours);

        // Dịch thuật AI nếu không phải tiếng Việt (đọc ngôn ngữ từ Preferences mỗi lần để phản ánh thay đổi)
        var preferredLang = Preferences.Get("AppLanguage", "vi-VN");
        if (!preferredLang.StartsWith("vi", StringComparison.OrdinalIgnoreCase))
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var translator = new TranslationService();
                    string targetLang = GetShortLangCode(preferredLang);

                    foreach (var p in allPois)
                    {
                        var dn = await translator.TranslateAsync(p.Name, targetLang);
                        if (!string.IsNullOrEmpty(dn)) p.DisplayName = dn;

                        var dc = await translator.TranslateAsync(p.Category, targetLang);
                        if (!string.IsNullOrEmpty(dc)) p.DisplayCategory = dc;
                    }

                    foreach (var t in allTours)
                    {
                        var dt = await translator.TranslateAsync(t.TourName, targetLang);
                        if (!string.IsNullOrEmpty(dt)) t.DisplayName = dt;
                    }

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        FeaturedPoisList.ItemsSource = new ObservableCollection<POI>(allPois);
                        ToursList.ItemsSource = new ObservableCollection<Itinerary>(allTours);
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Translation error: {ex.Message}");
                }
            });
        }
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