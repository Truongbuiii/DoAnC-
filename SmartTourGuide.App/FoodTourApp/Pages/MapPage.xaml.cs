using System.Collections.ObjectModel;
using FoodTourApp.Models;
using FoodTourApp.Services;
using Microsoft.Maui.Devices.Sensors;

#if ANDROID
using FoodTourApp.Platforms.Android;
#endif

namespace FoodTourApp.Pages;

public partial class MapPage : ContentPage
{
    private readonly DatabaseService _dbService;
    private readonly GeofenceService _geofenceService;
    private readonly NarrationService _narrationService;

    private bool _isTracking = false;
    private ObservableCollection<POI> _poiListForDisplay = new();
    private List<POI> _allPoisFromDb = new();
    private POI? _currentDisplayedPoi = null;

    // Lưu vị trí user hiện tại để vẽ lại blue dot sau khi map reload
    private double _lastUserLat = 0;
    private double _lastUserLon = 0;
    private bool _hasUserLocation = false;

    private readonly string[] _languageCodes = { "vi-VN", "en-US", "zh-CN", "ko-KR", "ja-JP" };
    private string _currentLanguage = "vi-VN";

#if ANDROID
    private AndroidTtsService? _androidTts;
#endif

    public MapPage()
    {
        InitializeComponent();

        _dbService = new DatabaseService();
        _geofenceService = new GeofenceService();
        _narrationService = new NarrationService();

        _geofenceService.CooldownMinutes = 5;
        _geofenceService.DebounceSeconds = 3;

        _narrationService.OnNarrationStarted += (s, e) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
                StatusLabel.Text = $"🔊 Đang phát ({GetLanguageFlag(e.LanguageCode)})");
        };

        _narrationService.OnNarrationCompleted += (s, e) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_currentDisplayedPoi != null)
                    StatusLabel.Text = $"📍 Cách {_currentDisplayedPoi.DistanceDisplay}";
            });
        };

        PoisListView.ItemsSource = _poiListForDisplay;

        _currentLanguage = Preferences.Get("AppLanguage", "vi-VN");
        int langIndex = Array.IndexOf(_languageCodes, _currentLanguage);
        if (langIndex >= 0) LanguagePicker.SelectedIndex = langIndex;
    }

    // ============================================================
    // TTS
    // ============================================================
    private void SpeakText(string text)
    {
#if ANDROID
        _androidTts?.SetLanguage(_currentLanguage);
        _androidTts?.Speak(text);
#endif
    }

    private void StopSpeaking()
    {
#if ANDROID
        _androidTts?.Stop();
#endif
    }

    private void OnLanguageChanged(object sender, EventArgs e)
    {
        if (LanguagePicker.SelectedIndex < 0) return;
        _currentLanguage = _languageCodes[LanguagePicker.SelectedIndex];
        _narrationService.CurrentLanguage = _currentLanguage;
        Preferences.Set("AppLanguage", _currentLanguage);
        if (_currentDisplayedPoi != null)
            SpeakText(_currentDisplayedPoi.GetDescription(_currentLanguage));
    }

    private string GetLanguageFlag(string code) => code switch
    {
        "vi-VN" => "🇻🇳",
        "en-US" => "🇺🇸",
        "zh-CN" => "🇨🇳",
        "ko-KR" => "🇰🇷",
        "ja-JP" => "🇯🇵",
        _ => "🌐"
    };

    // ============================================================
    // MAP HTML
    // ============================================================
    private string GenerateMapHtml(double centerLat, double centerLon, List<POI> allPois)
    {
        var markersJs = string.Join("\n", allPois.Select(p =>
        {
            string lat = p.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string lon = p.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string name = p.Name.Replace("'", "\\'");
            string cat = p.Category.Replace("'", "\\'");
            return $@"L.marker([{lat},{lon}]).addTo(map)
                .bindPopup('<b>{name}</b><br/>{cat}')
                .on('click', function() {{ window.location.href = 'poi://{p.PoiId}'; }});";
        }));

        string latStr = centerLat.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string lonStr = centerLon.ToString(System.Globalization.CultureInfo.InvariantCulture);

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'/>
    <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
    <style>* {{margin:0;padding:0}} #map {{height:100vh;width:100vw}}</style>
</head>
<body>
    <div id='map'></div>
    <script>
        var map = L.map('map').setView([{latStr},{lonStr}], 17);
        L.tileLayer('https://{{s}}.basemaps.cartocdn.com/rastertiles/voyager/{{z}}/{{x}}/{{y}}{{r}}.png', {{
            attribution: '&copy; OpenStreetMap &copy; CARTO',
            subdomains: 'abcd',
            maxZoom: 20
        }}).addTo(map);

        {markersJs}

        // Blue dot
        var userMarker = null;
        var userCircle = null;

        function setUserLocation(lat, lng, accuracy) {{
            if (userMarker) {{
                userMarker.setLatLng([lat, lng]);
                userCircle.setLatLng([lat, lng]);
                userCircle.setRadius(accuracy);
            }} else {{
                userMarker = L.circleMarker([lat, lng], {{
                    radius: 10,
                    fillColor: '#4A90E2',
                    color: 'white',
                    weight: 3,
                    fillOpacity: 1
                }}).addTo(map).bindPopup('📍 Vị trí của bạn');

                userCircle = L.circle([lat, lng], {{
                    radius: accuracy,
                    color: '#4A90E2',
                    fillColor: '#4A90E2',
                    fillOpacity: 0.1,
                    weight: 1
                }}).addTo(map);
            }}
        }}
    </script>
</body>
</html>";
    }

    // Gọi sau khi map load xong để vẽ lại blue dot
    private async Task DrawBlueDotAfterLoad(double lat, double lon)
    {
        await Task.Delay(800); // chờ map render xong
        string latStr = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string lonStr = lon.ToString(System.Globalization.CultureInfo.InvariantCulture);
        MainThread.BeginInvokeOnMainThread(() =>
            MapWebView.Eval($"setUserLocation({latStr}, {lonStr}, 15)"));
    }

    // ============================================================
    // LOGIC
    // ============================================================
    private async Task CheckAndNarrate(double lat, double lon)
    {
        _lastUserLat = lat;
        _lastUserLon = lon;
        _hasUserLocation = true;

        var result = _geofenceService.CheckGeofences(lat, lon, _allPoisFromDb);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateInterface(lat, lon, result.NearestPoi);
            RefreshListView();

            // Cập nhật blue dot
            string latStr = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string lonStr = lon.ToString(System.Globalization.CultureInfo.InvariantCulture);
            MapWebView.Eval($"setUserLocation({latStr}, {lonStr}, 15)");

            if (result.ShouldNarrate && result.PoiToNarrate != null)
                SpeakText(result.PoiToNarrate.GetDescription(_currentLanguage));
        });
    }

    private void RefreshListView()
    {
        var currentSearch = SearchEntry?.Text?.ToLower() ?? "";
        var filtered = _allPoisFromDb.Where(p => p.Name.ToLower().Contains(currentSearch)).ToList();
        _poiListForDisplay.Clear();
        foreach (var p in filtered) _poiListForDisplay.Add(p);
    }

    private void UpdateInterface(double lat, double lon, POI? poi = null)
    {
        MapLoading.IsVisible = false;

        if (poi != null)
        {
            _currentDisplayedPoi = poi;
            PoiCard.IsVisible = true;
            CurrentPoiName.Text = poi.Name;
            CategoryLabel.Text = poi.Category.ToUpper();
            PoiImage.Source = poi.FullImageUrl;
            StatusLabel.Text = $"📍 Cách {poi.DistanceDisplay}";
            BtnFavorite.Text = FavoritesPage.IsFavorite(poi.PoiId) ? "❤️ Đã lưu" : "🤍 Yêu thích";

            // Reload map focus vào POI, sau đó vẽ lại blue dot
            var htmlSource = new HtmlWebViewSource();
            htmlSource.Html = GenerateMapHtml(poi.Latitude, poi.Longitude, _allPoisFromDb);
            MapWebView.Source = htmlSource;

            // Vẽ lại blue dot sau khi map load
            if (_hasUserLocation)
                _ = DrawBlueDotAfterLoad(_lastUserLat, _lastUserLon);
        }
        else
        {
            StatusLabel.Text = $"📍 GPS: {lat:F5}, {lon:F5}";
            if (MapWebView.Source == null)
            {
                var htmlSource = new HtmlWebViewSource();
                htmlSource.Html = GenerateMapHtml(lat, lon, _allPoisFromDb);
                MapWebView.Source = htmlSource;
            }
        }
    }

    private void StartTrackingLocation()
    {
        if (_isTracking) return;
        _isTracking = true;

        Task.Run(async () =>
        {
            try
            {
                while (_isTracking)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                    var location = await Geolocation.Default.GetLocationAsync(request);
                    if (location != null) await CheckAndNarrate(location.Latitude, location.Longitude);
                    await Task.Delay(15000);
                }
            }
            catch { }
        });
    }

    // ============================================================
    // LIFECYCLE
    // ============================================================
    protected override async void OnAppearing()
    {
        base.OnAppearing();

#if ANDROID
        _androidTts = new AndroidTtsService();
        await _androidTts.InitializeAsync();
#endif

        MapWebView.Navigating += OnMapWebViewNavigating;

        var pois = await _dbService.GetPOIsAsync();
        _allPoisFromDb = pois.ToList();
        RefreshListView();

        var defaultHtml = new HtmlWebViewSource();
        defaultHtml.Html = GenerateMapHtml(10.75750, 106.70700, _allPoisFromDb);
        MapWebView.Source = defaultHtml;
        MapLoading.IsVisible = false;

        await Task.Delay(1500);
        StartTrackingLocation();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _isTracking = false;
        StopSpeaking();
        MapWebView.Navigating -= OnMapWebViewNavigating;
    }

    // ============================================================
    // MARKER CLICK từ JS
    // ============================================================
    private void OnMapWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (!e.Url.StartsWith("poi://")) return;
        e.Cancel = true;

        if (!int.TryParse(e.Url.Replace("poi://", ""), out int poiId)) return;
        var poi = _allPoisFromDb.FirstOrDefault(p => p.PoiId == poiId);
        if (poi == null) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            poi.DistanceDisplay = "Đã chọn";
            _currentDisplayedPoi = poi;
            PoiCard.IsVisible = true;
            CurrentPoiName.Text = poi.Name;
            CategoryLabel.Text = poi.Category.ToUpper();
            PoiImage.Source = poi.FullImageUrl;
            StatusLabel.Text = "📍 Đã chọn trên bản đồ";
            BtnFavorite.Text = FavoritesPage.IsFavorite(poi.PoiId) ? "❤️ Đã lưu" : "🤍 Yêu thích";
            MapLoading.IsVisible = false;
            // KHÔNG reload map — giữ nguyên markers và blue dot
            SpeakText(poi.GetDescription(_currentLanguage));
        });
    }

    // ============================================================
    // EVENT HANDLERS
    // ============================================================
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e) => RefreshListView();

    private void OnPoiSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not POI selectedPoi) return;
        _currentDisplayedPoi = selectedPoi;
        UpdateInterface(selectedPoi.Latitude, selectedPoi.Longitude, selectedPoi);
        SpeakText(selectedPoi.GetDescription(_currentLanguage));
    }

    private void OnToggleFavorite(object sender, EventArgs e)
    {
        if (_currentDisplayedPoi == null) return;
        if (FavoritesPage.IsFavorite(_currentDisplayedPoi.PoiId))
        {
            FavoritesPage.RemoveFavorite(_currentDisplayedPoi.PoiId);
            BtnFavorite.Text = "🤍 Yêu thích";
        }
        else
        {
            FavoritesPage.AddFavorite(_currentDisplayedPoi.PoiId);
            BtnFavorite.Text = "❤️ Đã lưu";
        }
    }

    private async void OnNavigateClicked(object sender, EventArgs e)
    {
        if (_currentDisplayedPoi == null)
        {
            await DisplayAlertAsync("Thông báo", "Chưa có địa điểm nào được chọn!", "OK");
            return;
        }
        try
        {
            var location = new Location(_currentDisplayedPoi.Latitude, _currentDisplayedPoi.Longitude);
            var options = new MapLaunchOptions { Name = _currentDisplayedPoi.Name, NavigationMode = NavigationMode.Walking };
            await Map.Default.OpenAsync(location, options);
        }
        catch
        {
            string latStr = _currentDisplayedPoi.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string lonStr = _currentDisplayedPoi.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            await Launcher.OpenAsync($"https://www.google.com/maps/dir/?api=1&destination={latStr},{lonStr}&travelmode=walking");
        }
    }

    private async void OnReplayClicked(object sender, EventArgs e)
    {
        if (_currentDisplayedPoi == null)
        {
            await DisplayAlertAsync("Thông báo", "Chưa có địa điểm nào được chọn!", "OK");
            return;
        }
        _geofenceService.ResetPoi(_currentDisplayedPoi.PoiId);
        SpeakText(_currentDisplayedPoi.GetDescription(_currentLanguage));
    }

    // ============================================================
    // SIMULATE
    // ============================================================
    private void SimulateOcOanh(object sender, EventArgs e)
    {
        var poi = _allPoisFromDb.FirstOrDefault(p => p.PoiId == 2);
        if (poi == null) return;
        poi.DistanceDisplay = "5 m";
        _currentDisplayedPoi = poi;

        // Lưu vị trí giả lập
        _lastUserLat = poi.Latitude;
        _lastUserLon = poi.Longitude;
        _hasUserLocation = true;

        UpdateInterface(poi.Latitude, poi.Longitude, poi);
        SpeakText(poi.GetDescription(_currentLanguage));
        // Blue dot sẽ tự vẽ trong DrawBlueDotAfterLoad gọi từ UpdateInterface
    }

    private void SimulateOcVu(object sender, EventArgs e)
    {
        var poi = _allPoisFromDb.FirstOrDefault(p => p.PoiId == 3);
        if (poi == null) return;
        poi.DistanceDisplay = "10 m";
        _currentDisplayedPoi = poi;

        _lastUserLat = poi.Latitude;
        _lastUserLon = poi.Longitude;
        _hasUserLocation = true;

        UpdateInterface(poi.Latitude, poi.Longitude, poi);
        SpeakText(poi.GetDescription(_currentLanguage));
    }

    private void SimulateReset(object sender, EventArgs e)
    {
        _geofenceService.ResetAll();
        _currentDisplayedPoi = null;
        _hasUserLocation = false;
        PoiCard.IsVisible = false;
        StopSpeaking();

        var defaultHtml = new HtmlWebViewSource();
        defaultHtml.Html = GenerateMapHtml(10.75750, 106.70700, _allPoisFromDb);
        MapWebView.Source = defaultHtml;
        StatusLabel.Text = "📍 Đã reset";
    }
}