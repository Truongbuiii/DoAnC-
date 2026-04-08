using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using FoodTourApp.Models;
using FoodTourApp.Services;

namespace FoodTourApp;

public partial class MainPage : ContentPage
{
    private readonly DatabaseService _dbService = new DatabaseService();
    private static bool _hasShownLanguagePicker = false; // static → chỉ hỏi 1 lần duy nhất

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Chỉ hỏi lần đầu tiên cài app (chưa có ngôn ngữ trong Preferences)
        if (!_hasShownLanguagePicker && !Preferences.ContainsKey("AppLanguage"))
        {
            _hasShownLanguagePicker = true;
            await Task.Delay(500); // chờ UI render xong

            string action = await DisplayActionSheet(
                "Chọn ngôn ngữ thuyết minh",
                null, null,
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

            // Cập nhật tab titles ngay sau khi chọn ngôn ngữ
            if (Shell.Current is AppShell appShell)
                appShell.ApplyLanguage();
        }

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
            await apiSync.SyncToursAsync();
            await apiSync.SyncLogsAsync();
            MainThread.BeginInvokeOnMainThread(async () =>
                await LoadData());
        });

        await LoadData();
    }

    private async Task LoadData()
    {
        var allPois = await _dbService.GetPOIsAsync();
        FeaturedPoisList.ItemsSource = new ObservableCollection<POI>(allPois);
        var tours = await _dbService.GetItinerariesAsync();
        ToursList.ItemsSource = new ObservableCollection<Itinerary>(tours);
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