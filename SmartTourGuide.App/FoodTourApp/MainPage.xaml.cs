using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using FoodTourApp.Models;
using FoodTourApp.Services;

namespace FoodTourApp;

public partial class MainPage : ContentPage
{
    private readonly DatabaseService _dbService = new DatabaseService();

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!Preferences.ContainsKey("AppLanguage"))
        {
            await Task.Delay(300);
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
        }

        await LoadDashboard();
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

        // Lưu danh sách POI của tour → MapPage sẽ đọc
        Preferences.Set("TourPoiIds", tour.PoiIdsRaw);
        Preferences.Set("TourName", tour.Name);

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