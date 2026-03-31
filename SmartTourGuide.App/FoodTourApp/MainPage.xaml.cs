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
        LoadDashboard();
    }

    private async void LoadDashboard()
    {
        var allPois = await _dbService.GetPOIsAsync();
        FeaturedPoisList.ItemsSource = new ObservableCollection<POI>(allPois);
    }

    private async void OnBannerTapped(object sender, EventArgs e)
    {
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