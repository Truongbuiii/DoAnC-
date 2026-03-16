using FoodTourApp.Models;
using FoodTourApp.Services;

namespace FoodTourApp.Pages;

public partial class MapPage : ContentPage
{
    private readonly DatabaseService _dbService;
    private string _lastPlayedPoi = string.Empty;
    private bool _isFirstLoad = true;

    public MapPage(DatabaseService dbService)
    {
        InitializeComponent();
        _dbService = dbService;

        // Bắt đầu theo dõi vị trí thực tế khi mở app
        StartTrackingLocation();
    }

    // 🛰️ BỘ MÁY THEO DÕI GPS THỰC TẾ
    private async void StartTrackingLocation()
    {
        try
        {
            PermissionStatus status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted) return;

            while (true)
            {
                try
                {
                    // Tăng lên 15 giây để Windows có đủ thời gian tìm vị trí
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(15));
                    var location = await Geolocation.Default.GetLocationAsync(request);

                    if (location != null)
                    {
                        UpdateInterface(location.Latitude, location.Longitude);
                        await CheckAndNarrate(location.Latitude, location.Longitude);
                    }
                }
                catch (TaskCanceledException)
                {
                    // Nếu hết thời gian mà chưa có vị trí, chỉ hiện thông báo chứ không làm văng App
                    MainThread.BeginInvokeOnMainThread(() => {
                        StatusLabel.Text = "Tín hiệu GPS yếu, đang tìm lại...";
                    });
                }

                await Task.Delay(10000); // Đợi 10 giây rồi thử lại
            }
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() => {
                StatusLabel.Text = "Lỗi hệ thống GPS. Hãy dùng các nút giả lập.";
            });
        }
    }

    private void UpdateInterface(double lat, double lon, POI poi = null)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // SỬA TẠI ĐÂY: Dùng định dạng Search API để tự động cắm Pin mà không bị lỗi iframe
            string googleMapsUrl = $"https://www.google.com/maps/search/?api=1&query={lat},{lon}";

            // Cập nhật bản đồ (Bỏ điều kiện _isFirstLoad để mỗi lần nhấn nút bản đồ đều nhảy)
            MapWebView.Source = googleMapsUrl;

            // Ẩn loading
            MapLoading.IsRunning = false;
            MapLoading.IsVisible = false;

            // Cập nhật tọa độ trên nhãn
            StatusLabel.Text = $"📍 Vị trí: {lat:F5}, {lon:F5}";

            // Nếu có POI (khi nhấn nút giả lập hoặc đi vào vùng quán ăn)
            if (poi != null)
            {
                CurrentPoiName.Text = poi.Name;
                CategoryLabel.Text = poi.Category;
                PoiImage.Source = poi.ImageSource;
            }
        });
    }
    // 🧠 NARRATION ENGINE (Logic thuyết minh)
    private async Task CheckAndNarrate(double lat, double lon)
    {
        var userLocation = new Location(lat, lon);
        var pois = await _dbService.GetPOIsAsync();

        foreach (var poi in pois)
        {
            double distance = Location.CalculateDistance(userLocation, poi.Latitude, poi.Longitude, DistanceUnits.Kilometers);

            // Chuyển mét sang km để so sánh
            double radiusInKm = poi.TriggerRadius / 1000.0;

            if (distance <= radiusInKm)
            {
                if (_lastPlayedPoi != poi.Name)
                {
                    _lastPlayedPoi = poi.Name;

                    // CẬP NHẬT GIAO DIỆN
                    StatusLabel.Text = $"📍 Bạn đang tại: {poi.Name}";
                    CategoryLabel.Text = poi.Category;
                    PoiImage.Source = poi.ImageSource; // Hiển thị ảnh tương ứng

                    // THUYẾT MINH
                    await TextToSpeech.Default.SpeakAsync(poi.Description);
                }
                return;
            }
        }
    }

    // --- CÁC HÀM GIẢ LẬP DI CHUYỂN (BẮT BUỘC PHẢI ĐÚNG TÊN VÀ THAM SỐ) ---

    private async void SimulateCongChao(object sender, EventArgs e)
    {
        await CheckAndNarrate(10.75750, 106.70700);
    }

    private async void SimulateOcOanh(object sender, EventArgs e)
    {
        await CheckAndNarrate(10.75883, 106.70505);
    }

    private async void SimulateOcVu(object sender, EventArgs e)
    {
        await CheckAndNarrate(10.75916, 106.70452);
    }

    private async void SimulateLauBo(object sender, EventArgs e)
    {
        await CheckAndNarrate(10.75822, 106.70611);
    }

    private async void SimulatePhaLau(object sender, EventArgs e)
    {
        await CheckAndNarrate(10.75940, 106.70410);
    }

    private async void SimulateSushi(object sender, EventArgs e)
    {
        await CheckAndNarrate(10.75800, 106.70650);
    }

    private void SimulateReset(object sender, EventArgs e)
    {
        _lastPlayedPoi = string.Empty;
        StatusLabel.Text = "Đã reset thuyết minh.";
        PoiImage.Source = "cong_chao.jpg";
        CategoryLabel.Text = "Ăn vặt";
        CurrentPoiName.Text = "Phố ẩm thực Vĩnh Khánh";
    }
}