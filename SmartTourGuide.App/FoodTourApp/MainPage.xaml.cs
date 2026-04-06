using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using FoodTourApp.Models;
using FoodTourApp.Services;

namespace FoodTourApp;

public partial class MainPage : ContentPage
{
    private readonly DatabaseService _dbService = new DatabaseService();
    private bool _isFirstLoad = true;

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isFirstLoad && !Preferences.ContainsKey("AppLanguage"))
        {
            await Task.Delay(300);
            string action = await DisplayActionSheet(
                Lang.Get("select_language"),
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
        }

        _isFirstLoad = false;
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