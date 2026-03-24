using System.Collections.ObjectModel;
using FoodTourApp.Models;
using FoodTourApp.Services;
using Microsoft.Maui.Devices.Sensors;

namespace FoodTourApp.Pages;

public partial class MapPage : ContentPage
{
    private readonly DatabaseService _dbService;
    private string _lastPlayedPoi = string.Empty;
    private CancellationTokenSource _ttsCancelSource;
    private bool _isTracking = false;
    private ObservableCollection<POI> _poiListForDisplay = new ObservableCollection<POI>();
    private List<POI> _allPoisFromDb = new List<POI>();

    // Lưu POI hiện tại để dùng cho Navigate và Replay
    private POI _currentDisplayedPoi = null;

    public MapPage()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
        _ttsCancelSource = new CancellationTokenSource();
        PoisListView.ItemsSource = _poiListForDisplay;
    }

    // ============================================================
    // SỬA LỖI: Tạo HTML chứa Google Maps Embed (tránh lỗi intent://)
    // ============================================================
    private string GenerateMapHtml(double lat, double lon, string poiName = "Vị trí hiện tại")
    {
        // Dùng InvariantCulture để đảm bảo dấu chấm thập phân (10.75 thay vì 10,75)
        string latStr = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string lonStr = lon.ToString(System.Globalization.CultureInfo.InvariantCulture);

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no'>
    <style>
        * {{ margin: 0; padding: 0; }}
        html, body {{ height: 100%; width: 100%; }}
        #map {{ height: 100%; width: 100%; border: none; }}
    </style>
</head>
<body>
    <iframe 
        id='map'
        src='https://maps.google.com/maps?q={latStr},{lonStr}&z=17&output=embed&hl=vi'
        frameborder='0' 
        style='width:100%; height:100%;'
        allowfullscreen>
    </iframe>
</body>
</html>";
    }

    private async Task CheckAndNarrate(double lat, double lon)
    {
        var userLocation = new Location(lat, lon);
        POI closestPoi = null;

        foreach (var poi in _allPoisFromDb)
        {
            double distKm = Location.CalculateDistance(userLocation, poi.Latitude, poi.Longitude, DistanceUnits.Kilometers);
            double meters = distKm * 1000;
            poi.DistanceDisplay = meters < 1000 ? $"{meters:F0} m" : $"{distKm:F1} km";

            double radiusInKm = poi.TriggerRadius / 1000.0;
            if (distKm <= radiusInKm && _lastPlayedPoi != poi.Name)
            {
                _lastPlayedPoi = poi.Name;
                closestPoi = poi;
            }
        }

        // CHỈ cập nhật UI trên MainThread để tránh treo App
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            UpdateInterface(lat, lon, closestPoi);
            if (closestPoi != null)
            {
                await SpeakDescription(closestPoi.Description);
            }
            RefreshListView();
        });
    }

    private void RefreshListView()
    {
        var currentSearch = SearchEntry?.Text?.ToLower() ?? "";
        var filtered = _allPoisFromDb
            .Where(p => p.Name.ToLower().Contains(currentSearch))
            .ToList();

        _poiListForDisplay.Clear();
        foreach (var p in filtered) _poiListForDisplay.Add(p);
    }

    private void UpdateInterface(double lat, double lon, POI poi = null)
    {
        MapLoading.IsVisible = false;

        if (poi != null)
        {
            // Lưu POI hiện tại
            _currentDisplayedPoi = poi;

            PoiCard.IsVisible = true;
            CurrentPoiName.Text = poi.Name;
            CategoryLabel.Text = poi.Category.ToUpper();
            PoiImage.Source = poi.FullImageUrl;
            StatusLabel.Text = "Bạn đang ở gần: " + poi.Name;

            // SỬA LỖI: Dùng HTML Embed thay vì URL trực tiếp
            var htmlSource = new HtmlWebViewSource();
            htmlSource.Html = GenerateMapHtml(poi.Latitude, poi.Longitude, poi.Name);
            MapWebView.Source = htmlSource;
        }
        else
        {
            StatusLabel.Text = $"📍 GPS: {lat:F5}, {lon:F5}";

            // SỬA LỖI: Dùng HTML Embed thay vì URL trực tiếp
            if (MapWebView.Source == null)
            {
                var htmlSource = new HtmlWebViewSource();
                htmlSource.Html = GenerateMapHtml(lat, lon);
                MapWebView.Source = htmlSource;
            }
        }
    }

    private void StartTrackingLocation()
    {
        if (_isTracking) return;
        _isTracking = true;

        // Tách hẳn luồng GPS ra khỏi UI Thread để không gây "isn't responding"
        Task.Run(async () =>
        {
            try
            {
                while (_isTracking)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                    var location = await Geolocation.Default.GetLocationAsync(request);

                    if (location != null)
                    {
                        await CheckAndNarrate(location.Latitude, location.Longitude);
                    }
                    await Task.Delay(15000); // 15 giây/lần cho nhẹ máy
                }
            }
            catch { }
        });
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var pois = await _dbService.GetPOIsAsync();
        _allPoisFromDb = pois.ToList();
        RefreshListView();

        // SỬA LỖI: Load bản đồ mặc định ngay khi mở trang (dùng HTML Embed)
        var defaultHtml = new HtmlWebViewSource();
        defaultHtml.Html = GenerateMapHtml(10.75750, 106.70700, "Phố ẩm thực Vĩnh Khánh");
        MapWebView.Source = defaultHtml;
        MapLoading.IsVisible = false;

        await Task.Delay(1500); // Chờ UI ổn định rồi mới bật GPS
        StartTrackingLocation();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _isTracking = false;
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e) => RefreshListView();

    private async void OnPoiSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is POI selectedPoi)
        {
            // Lưu POI được chọn
            _currentDisplayedPoi = selectedPoi;

            UpdateInterface(selectedPoi.Latitude, selectedPoi.Longitude, selectedPoi);
            await SpeakDescription(selectedPoi.Description);
        }
    }

    // ============================================================
    // Event Handlers cho các nút trên PoiCard
    // ============================================================

    /// <summary>
    /// Mở Google Maps app để dẫn đường đến POI hiện tại
    /// </summary>
    private async void OnNavigateClicked(object sender, EventArgs e)
    {
        if (_currentDisplayedPoi == null)
        {
            await DisplayAlert("Thông báo", "Chưa có địa điểm nào được chọn!", "OK");
            return;
        }

        try
        {
            // Mở Google Maps app (nếu có cài)
            var location = new Location(_currentDisplayedPoi.Latitude, _currentDisplayedPoi.Longitude);
            var options = new MapLaunchOptions
            {
                Name = _currentDisplayedPoi.Name,
                NavigationMode = NavigationMode.Walking
            };

            await Map.Default.OpenAsync(location, options);
        }
        catch
        {
            // Fallback - Mở trình duyệt nếu không có app Maps
            string latStr = _currentDisplayedPoi.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string lonStr = _currentDisplayedPoi.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string url = $"https://www.google.com/maps/dir/?api=1&destination={latStr},{lonStr}&travelmode=walking";
            await Launcher.OpenAsync(url);
        }
    }

    /// <summary>
    /// Phát lại thuyết minh (TTS) cho POI hiện tại
    /// </summary>
    private async void OnReplayClicked(object sender, EventArgs e)
    {
        if (_currentDisplayedPoi == null)
        {
            await DisplayAlert("Thông báo", "Chưa có địa điểm nào được chọn!", "OK");
            return;
        }

        // Phát lại TTS cho POI hiện tại
        await SpeakDescription(_currentDisplayedPoi.Description);
    }

    // ============================================================
    // Các nút Simulate để test
    // ============================================================

    private async void SimulateOcOanh(object sender, EventArgs e) => await CheckAndNarrate(10.75883, 106.70505);
    private async void SimulateOcVu(object sender, EventArgs e) => await CheckAndNarrate(10.75916, 106.70452);

    private async void SimulateReset(object sender, EventArgs e)
    {
        _lastPlayedPoi = string.Empty;
        _currentDisplayedPoi = null;
        PoiCard.IsVisible = false;
        await CheckAndNarrate(10.75750, 106.70700);
    }

    private async Task SpeakDescription(string text)
    {
        try
        {
            if (_ttsCancelSource != null) _ttsCancelSource.Cancel();
            _ttsCancelSource = new CancellationTokenSource();
            await TextToSpeech.Default.SpeakAsync(text, cancelToken: _ttsCancelSource.Token);
        }
        catch { }
    }
}