using System.Collections.ObjectModel;
using FoodTourApp.Models;
using FoodTourApp.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Networking;

namespace FoodTourApp;

public partial class MainPage : ContentPage
{
    private readonly DatabaseService _dbService = new DatabaseService();

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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var newLang = Preferences.Get("AppLanguage", "vi-VN");

        // Nếu đổi ngôn ngữ → reset cache để dịch lại
        if (newLang != _currentLanguage)
        {
            _currentLanguage = newLang;
            _lastTranslatedLang = "";
            _isDataLoaded = false;
        }

        ApplyLanguage();
        LoadInitialData();
    }

    private void LoadInitialData()
    {
        if (_isDataLoaded)
        {
            UpdateUI();
            if (!_currentLanguage.StartsWith("vi") && _lastTranslatedLang != _currentLanguage)
                _ = TranslateDataAsync();
            return;
        }

        _isDataLoaded = true;

        _ = Task.Run(async () =>
        {
            var pois = (await _dbService.GetPOIsAsync()).ToList();
            var tours = (await _dbService.GetItinerariesAsync()).ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                _cachedPois = pois;
                _cachedTours = tours;
                UpdateUI();
                if (!_currentLanguage.StartsWith("vi"))
                    _ = TranslateDataAsync();
            });
        });

        if (_isInitialLoad)
        {
            _ = RunBackgroundSync();
            _isInitialLoad = false;
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
        if (_lastTranslatedLang == _currentLanguage) return;
        string targetLang = _currentLanguage;

        try
        {
            var translator = new TranslationService();
            string shortCode = GetShortLangCode(targetLang);

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

            _lastTranslatedLang = targetLang;
            MainThread.BeginInvokeOnMainThread(() => UpdateUI());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi dịch MainPage: {ex.Message}");
            foreach (var p in _cachedPois) { p.DisplayName = p.Name; p.DisplayCategory = p.Category; }
            foreach (var t in _cachedTours) { t.DisplayName = t.TourName; }
            MainThread.BeginInvokeOnMainThread(() => UpdateUI());
        }
    }

    private async Task RunBackgroundSync()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet) return;

        var apiSync = new ApiSyncService(_dbService);
        await Task.Run(async () =>
        {
            await apiSync.SyncPoisAsync();
            await apiSync.SyncToursAsync();
            await apiSync.SyncLogsAsync();
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