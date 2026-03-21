using System.Collections.ObjectModel;
using FoodTourApp.Models;
using FoodTourApp.Services;

namespace FoodTourApp.Pages;

public partial class MapPage : ContentPage
{
    private readonly DatabaseService _dbService;
    private string _lastPlayedPoi = string.Empty;
    private CancellationTokenSource _ttsCancelSource;
    private List<POI> _allPois = new List<POI>();
    private bool _hasLocation = false; // Kiểm tra xem đã hiện bản đồ chưa

    public MapPage()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
        _ttsCancelSource = new CancellationTokenSource();

        // Bước 1: Hiện ngay bản đồ tại Cổng chào để không bị màn hình trắng
        UpdateInterface(10.75750, 106.70700);

        // Bước 2: Bắt đầu dò tìm GPS thực
        StartTrackingLocation();
    }

    // --- 🎙️ NARRATION ENGINE (TTS) ---

    private async Task SpeakDescription(string text)
    {
        try
        {
            if (_ttsCancelSource != null) _ttsCancelSource.Cancel();
            _ttsCancelSource = new CancellationTokenSource();

            await TextToSpeech.Default.SpeakAsync(text, cancelToken: _ttsCancelSource.Token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { Console.WriteLine($"Lỗi TTS: {ex.Message}"); }
    }

    private async Task CheckAndNarrate(double lat, double lon)
    {
        var userLocation = new Location(lat, lon);
        var pois = await _dbService.GetPOIsAsync();

        foreach (var poi in pois)
        {
            double distance = Location.CalculateDistance(userLocation, poi.Latitude, poi.Longitude, DistanceUnits.Kilometers);
            double radiusInKm = poi.TriggerRadius / 1000.0;

            if (distance <= radiusInKm)
            {
                if (_lastPlayedPoi != poi.Name)
                {
                    _lastPlayedPoi = poi.Name;
                    UpdateInterface(lat, lon, poi);
                    await SpeakDescription(poi.Description);
                }
                return;
            }
        }
        // Nếu không ở gần quán nào, vẫn cập nhật tọa độ trên bản đồ
        UpdateInterface(lat, lon);
    }

    // --- 🗺️ INTERFACE & MAP UPDATE ---

    private void UpdateInterface(double lat, double lon, POI poi = null)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // TẮT VÒNG XOAY NGAY LẬP TỨC
            MapLoading.IsRunning = false;
            MapLoading.IsVisible = false;

            // DÙNG URL CHÍNH THỨC CỦA GOOGLE MAPS SEARCH (Ổn định nhất)
            string googleMapsUrl = $"https://www.google.com/maps/search/?api=1&query={lat},{lon}";

            // Chỉ load lại WebView nếu vị trí thay đổi đáng kể để tránh nháy màn hình
            if (MapWebView.Source == null || poi != null)
            {
                MapWebView.Source = googleMapsUrl;
            }

            if (poi != null)
            {
                PoiCard.IsVisible = true;
                CurrentPoiName.Text = poi.Name;
                CategoryLabel.Text = poi.Category.ToUpper();
                PoiImage.Source = poi.ImageSource;
                StatusLabel.Text = "Đã xác định vị trí";
            }
            else
            {
                StatusLabel.Text = $"📍 {lat:F5}, {lon:F5}";
            }
        });
    }

    // --- 🛰️ GPS TRACKING (Có xử lý lỗi treo) ---

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
                    // Tăng timeout lên 10s để tránh văng trên Windows
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                    var location = await Geolocation.Default.GetLocationAsync(request);

                    if (location != null)
                    {
                        await CheckAndNarrate(location.Latitude, location.Longitude);
                    }
                }
                catch { /* Bỏ qua lỗi bắt sóng yếu để vòng lặp tiếp tục */ }

                await Task.Delay(10000);
            }
        }
        catch (Exception) { }
    }

    // --- 🔍 TÌM KIẾM & CHỌN ĐỊA ĐIỂM ---

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.ToLower() ?? string.Empty;
        var filtered = _allPois.Where(p => p.Name.ToLower().Contains(searchText) || p.Category.ToLower().Contains(searchText)).ToList();
        PoisListView.ItemsSource = new ObservableCollection<POI>(filtered);
    }

    private async void OnPoiSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is POI selectedPoi)
        {
            UpdateInterface(selectedPoi.Latitude, selectedPoi.Longitude, selectedPoi);
            await SpeakDescription(selectedPoi.Description);
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var allPoisFromDb = await _dbService.GetPOIsAsync();
        _allPois = allPoisFromDb.ToList();
        PoisListView.ItemsSource = new ObservableCollection<POI>(_allPois);
    }

    // --- 🔘 NÚT BẤM GIẢ LẬP ---

    private async void SimulateOcOanh(object sender, EventArgs e) => await CheckAndNarrate(10.75883, 106.70505);
    private async void SimulateOcVu(object sender, EventArgs e) => await CheckAndNarrate(10.75916, 106.70452);
    private async void SimulateLauBo(object sender, EventArgs e) => await CheckAndNarrate(10.75822, 106.70611);
    private async void SimulateReset(object sender, EventArgs e)
    {
        _lastPlayedPoi = string.Empty;
        UpdateInterface(10.75750, 106.70700);
    }
    private async void OnReplayClicked(object sender, EventArgs e)
    {
        var currentPoi = _allPois.FirstOrDefault(p => p.Name == CurrentPoiName.Text);
        if (currentPoi != null) await SpeakDescription(currentPoi.Description);
    }
    private async void OnNavigateClicked(object sender, EventArgs e)
    {
        var currentPoi = _allPois.FirstOrDefault(p => p.Name == CurrentPoiName.Text);
        if (currentPoi != null) await Launcher.OpenAsync($"https://www.google.com/maps/dir/?api=1&destination={currentPoi.Latitude},{currentPoi.Longitude}");
    }
}