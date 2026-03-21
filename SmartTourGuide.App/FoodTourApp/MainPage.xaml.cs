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
        // 1. Nạp dữ liệu Tour mẫu
        ToursCollection.ItemsSource = new ObservableCollection<Itinerary>
        {
            new Itinerary { Name = "Tour Ốc Huyền Thoại", ImageSource = "ocoanh.jpg", Duration = "⏱️ 90 phút | 📍 3 điểm dừng", PoiIds = new List<int>{1, 2, 3} },
            new Itinerary { Name = "Tour Ăn Vặt Xế Chiều", ImageSource = "phaslau.jpg", Duration = "⏱️ 60 phút | 📍 4 điểm dừng", PoiIds = new List<int>{4, 5, 6} }
        };

        // 2. Nạp dữ liệu Quán từ SQLite
        var pois = await _dbService.GetPOIsAsync();
        PoisCollection.ItemsSource = new ObservableCollection<POI>(pois.Take(10));
    }

    // Khi chọn Tour -> Chuyển sang Tab Bản đồ
    private async void OnTourSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Itinerary selectedTour)
        {
            // Logic lọc tour sẽ viết ở đây trong Phase 4
            await Shell.Current.GoToAsync("//MapPage");
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    // Khi chọn Quán -> Sang trang Chi tiết
    private async void OnPoiSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is POI selectedPoi)
        {
            await Navigation.PushAsync(new Pages.PoiDetailPage(selectedPoi));
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}