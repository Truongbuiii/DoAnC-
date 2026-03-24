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
        // 1. Tạo dữ liệu Tour mẫu (Ảnh phải có trong Resources/Images)
        ToursList.ItemsSource = new ObservableCollection<Itinerary>
        {
            new Itinerary { Name = "Tour Ốc Huyền Thoại", ImageSource = "ocoanh.jpg", Duration = "⏱️ 90 phút | 📍 3 điểm", PoiIds = new List<int>{1, 2, 3} },
            new Itinerary { Name = "Ăn Vặt Xế Chiều", ImageSource = "phaslau.jpg", Duration = "⏱️ 60 phút | 📍 4 điểm", PoiIds = new List<int>{4, 5, 6} }
        };

        // 2. Lấy danh sách quán từ SQLite hiện có
        var allPois = await _dbService.GetPOIsAsync();
        FeaturedPoisList.ItemsSource = new ObservableCollection<POI>(allPois);
    }

    private async void OnTourSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Itinerary selectedTour)
        {
            // Tạm thời chỉ chuyển sang tab Bản đồ
            await Shell.Current.GoToAsync("//MapPage");
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    private async void OnPoiSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is POI selectedPoi)
        {
            // Chuyển sang trang Chi tiết đã sửa lỗi hôm trước
            await Navigation.PushAsync(new Pages.PoiDetailPage(selectedPoi));
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}