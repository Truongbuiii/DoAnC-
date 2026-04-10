using System.Collections.ObjectModel;
using FoodTourApp.Models;
using FoodTourApp.Services;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;
using System.Globalization;


#if ANDROID
using Android.Content;
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

    private double _lastUserLat = 0;
    private double _lastUserLon = 0;
    private bool _hasUserLocation = false;

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

        _geofenceService.CooldownMinutes = 1;
        _geofenceService.DebounceSeconds = 1;

        // Đăng ký sự kiện Narration
        _narrationService.OnNarrationStarted += (s, e) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
                StatusLabel.Text = $"🔊 ({GetLanguageFlag(e.LanguageCode)})");
        };

        _narrationService.OnNarrationCompleted += (s, e) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_currentDisplayedPoi != null)
                    StatusLabel.Text = $"📍 {_currentDisplayedPoi.DistanceDisplay}";
            });
        };

        PoisListView.ItemsSource = _poiListForDisplay;
        _currentLanguage = Preferences.Get("AppLanguage", "vi-VN");
    }

    // --- CÁC HÀM TIỆN ÍCH (HELPER METHODS) ---

    private static bool IsVietnamese(string culture)
    {
        if (string.IsNullOrEmpty(culture)) return false;
        return culture.ToLower().StartsWith("vi");
    }

    private static string GetShortLangCode(string culture)
    {
        if (string.IsNullOrEmpty(culture)) return string.Empty;
        culture = culture.ToLower();
        if (culture.StartsWith("en")) return "en";
        if (culture.StartsWith("ja")) return "ja";
        if (culture.StartsWith("ko")) return "ko";
        if (culture.StartsWith("zh")) return "zh";
        return string.Empty;
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

    private void ApplyLanguage()
    {
        Title = Lang.Get("tab_map");
        SearchEntry.Placeholder = Lang.Get("map_search");
        BtnNavigate.Text = Lang.Get("map_navigate");
        BtnReplay.Text = Lang.Get("map_replay");
        BtnDetail.Text = Lang.Get("map_detail");

        if (_currentDisplayedPoi != null)
        {
            BtnFavorite.Text = FavoritesPage.IsFavorite(_currentDisplayedPoi.PoiId)
                ? Lang.Get("map_favorited") : Lang.Get("map_favorite");
        }
        else
        {
            BtnFavorite.Text = Lang.Get("map_favorite");
        }
    }

    // --- XỬ LÝ ÂM THANH ---

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

    // --- XỬ LÝ BẢN ĐỒ (WEBVIEW) ---

    private string GenerateMapHtml(double centerLat, double centerLon, List<POI> allPois, int highlightPoiId = -1)
    {
        // 1. Xử lý Markers (Dùng nội suy chuỗi cẩn thận)
        var markersJs = string.Join("\n", allPois.Select(p =>
        {
            string lat = p.Latitude.ToString(CultureInfo.InvariantCulture);
            string lon = p.Longitude.ToString(CultureInfo.InvariantCulture);
            string name = p.Name.Replace("'", "\\'");
            string cat = p.Category.Replace("'", "\\'");
            bool isHighlight = p.PoiId == highlightPoiId;
            string color = isHighlight ? "#ff9800" : "#4caf50";
            int size = isHighlight ? 36 : 28;

            return $@"
        var icon_{p.PoiId} = L.divIcon({{
            className: 'custom-div-icon',
            html: '<div style=""width:{size}px;height:{size}px;background:{color};border-radius:50% 50% 50% 0;transform:rotate(-45deg);border:2px solid white;box-shadow:0 2px 4px rgba(0,0,0,0.3)""></div>',
            iconSize: [{size}, {size}],
            iconAnchor: [{size / 2}, {size}]
        }});
        var marker_{p.PoiId} = L.marker([{lat}, {lon}], {{ icon: icon_{p.PoiId} }})
            .bindPopup('<b>{name}</b><br/>{cat}')
            .on('click', function() {{ window.location.href = 'poi://{p.PoiId}'; }});
        {(isHighlight ? $"marker_{p.PoiId}.openPopup();" : "")}
        markerCluster.addLayer(marker_{p.PoiId});";
        }));

        // 2. Tọa độ trung tâm
        string cLat = centerLat.ToString(CultureInfo.InvariantCulture);
        string cLon = centerLon.ToString(CultureInfo.InvariantCulture);

        // 3. Khối HTML (Dùng @ nhưng KHÔNG dùng $ để tránh lỗi dấu ngoặc của Leaflet/CSS)
        string htmlTemplate = @"
<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no' />
    <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'/>
    <link rel='stylesheet' href='https://unpkg.com/leaflet.markercluster@1.5.3/dist/MarkerCluster.css'/>
    <link rel='stylesheet' href='https://unpkg.com/leaflet.markercluster@1.5.3/dist/MarkerCluster.Default.css'/>
    <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
    <script src='https://unpkg.com/leaflet.markercluster@1.5.3/dist/leaflet.markercluster.js'></script>
    <style>
        body, html, #map { height: 100%; width: 100%; margin: 0; padding: 0; }
        .custom-div-icon { background: none; border: none; }
    </style>
</head>
<body>
    <div id='map'></div>
    <script>
        var map = L.map('map', { zoomControl: false }).setView([CENTER_LAT, CENTER_LON], 17);
        
        L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png', {
            attribution: '&copy; OpenStreetMap &copy; CARTO',
            subdomains: 'abcd',
            maxZoom: 20
        }).addTo(map);
        
        var markerCluster = L.markerClusterGroup();
        MARKERS_HERE
        map.addLayer(markerCluster);

        var userMarker = null, userCircle = null;
        function setUserLocation(lat, lng, acc) {
            if (userMarker) {
                userMarker.setLatLng([lat, lng]);
                userCircle.setLatLng([lat, lng]).setRadius(acc);
            } else {
                userMarker = L.circleMarker([lat, lng], { radius: 8, fillColor: '#2196f3', color: 'white', weight: 2, fillOpacity: 1 }).addTo(map);
                userCircle = L.circle([lat, lng], { radius: acc, color: '#2196f3', weight: 1, fillOpacity: 0.1 }).addTo(map);
            }
        }
    </script>
</body>
</html>";

        // 4. Thay thế các placeholder bằng dữ liệu thật (Cực kỳ an toàn, không lo lỗi cú pháp C#)
        return htmlTemplate.Replace("CENTER_LAT", cLat)
                            .Replace("CENTER_LON", cLon)
                            .Replace("MARKERS_HERE", markersJs);
    }

    private async Task DrawBlueDotAfterLoad(double lat, double lon)
    {
        await Task.Delay(800);
        string latStr = lat.ToString(CultureInfo.InvariantCulture);
        string lonStr = lon.ToString(CultureInfo.InvariantCulture);
        MainThread.BeginInvokeOnMainThread(() =>
            MapWebView.Eval($"setUserLocation({latStr}, {lonStr}, 15)"));
    }

    // --- ĐỊNH VỊ VÀ GEOFENCING ---

    private void StartTrackingLocation()
    {
        if (_isTracking) return;
        _isTracking = true;

#if ANDROID
        LocationForegroundService.LocationUpdated += OnLocationUpdated;
        var intent = new Intent(Platform.CurrentActivity, typeof(LocationForegroundService));
        Platform.CurrentActivity?.StartForegroundService(intent);
#endif
    }

    private void StopTrackingLocation()
    {
        if (!_isTracking) return;
        _isTracking = false;

#if ANDROID
        LocationForegroundService.LocationUpdated -= OnLocationUpdated;
        var intent = new Intent(Platform.CurrentActivity, typeof(LocationForegroundService));
        Platform.CurrentActivity?.StopService(intent);
#endif
    }

#if ANDROID
    private async void OnLocationUpdated(object? sender, Location location)
    {
        await CheckAndNarrate(location.Latitude, location.Longitude);
    }
#endif

    private async Task CheckAndNarrate(double lat, double lon)
    {
        _lastUserLat = lat;
        _lastUserLon = lon;
        _hasUserLocation = true;

        var result = _geofenceService.CheckGeofences(lat, lon, _allPoisFromDb);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            string latStr = lat.ToString(CultureInfo.InvariantCulture);
            string lonStr = lon.ToString(CultureInfo.InvariantCulture);
            MapWebView.Eval($"setUserLocation({latStr}, {lonStr}, 15)");

            RefreshListView();

            if (result.NearestPoi != null)
            {
                var poi = result.NearestPoi;
                _currentDisplayedPoi = poi;
                PoiCard.IsVisible = true;
                CurrentPoiName.Text = poi.Name;
                CategoryLabel.Text = poi.Category.ToUpper();
                PoiImage.Source = poi.FullImageUrl;
                StatusLabel.Text = $"📍 {poi.DistanceDisplay}";
                BtnFavorite.Text = FavoritesPage.IsFavorite(poi.PoiId)
                    ? Lang.Get("map_favorited") : Lang.Get("map_favorite");
                MapLoading.IsVisible = false;
            }
            else
            {
                StatusLabel.Text = $"📍 GPS: {lat:F5}, {lon:F5}";
            }

            if (result.ShouldNarrate && result.PoiToNarrate != null)
            {
                _ = Task.Run(async () =>
                {
                    var poi = result.PoiToNarrate;
                    string textToSpeak;
                    if (IsVietnamese(_currentLanguage)) textToSpeak = poi.DescriptionVi;
                    else
                    {
                        var translator = new TranslationService();
                        var shortCode = GetShortLangCode(_currentLanguage);
                        var translated = await translator.TranslateAsync(poi.DescriptionVi, shortCode);
                        textToSpeak = string.IsNullOrEmpty(translated) ? poi.DescriptionVi : translated;
                    }

                    MainThread.BeginInvokeOnMainThread(() => SpeakText(textToSpeak));
                });

                _ = _dbService.LogActivityAsync(result.PoiToNarrate.PoiId, "AutoTrigger", _currentLanguage);
            }
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
            StatusLabel.Text = $"📍 {poi.DistanceDisplay}";
            BtnFavorite.Text = FavoritesPage.IsFavorite(poi.PoiId)
                ? Lang.Get("map_favorited") : Lang.Get("map_favorite");

            var htmlSource = new HtmlWebViewSource();
            htmlSource.Html = GenerateMapHtml(poi.Latitude, poi.Longitude, _allPoisFromDb, poi.PoiId);
            MapWebView.Source = htmlSource;

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

    // --- VÒNG ĐỜI TRANG (LIFECYCLE) ---

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        Lang.Load();
        _currentLanguage = Preferences.Get("AppLanguage", "vi-VN");
        ApplyLanguage();

        var apiSync = new ApiSyncService(_dbService);
        _ = Task.Run(async () =>
        {
            await apiSync.SyncPoisAsync();
            await apiSync.SyncToursAsync();
            await apiSync.SyncAudiosAsync();
            await apiSync.SyncLogsAsync();
            await _dbService.TranslateAndCachePoisAsync();

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                _allPoisFromDb = (await _dbService.GetPOIsAsync()).ToList();
                RefreshListView();
            });
        });

#if ANDROID
        _androidTts = new AndroidTtsService();
        await _androidTts.InitializeAsync();
#endif

        MapWebView.Navigating += OnMapWebViewNavigating;

        var pois = await _dbService.GetPOIsAsync();
        _allPoisFromDb = pois.ToList();
        RefreshListView();

        // Xử lý Tour hoặc Highlight từ Preferences
        if (HandlePreferences()) return;

        var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        if (status == PermissionStatus.Granted)
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5));
                var location = await Geolocation.Default.GetLocationAsync(request);
                if (location != null)
                {
                    _lastUserLat = location.Latitude;
                    _lastUserLon = location.Longitude;
                    _hasUserLocation = true;
                    UpdateInterface(location.Latitude, location.Longitude);
                    await DrawBlueDotAfterLoad(location.Latitude, location.Longitude);
                }
                else LoadDefaultMap();
            }
            catch { LoadDefaultMap(); }
            StartTrackingLocation();
        }
        else
        {
            LoadDefaultMap();
            StatusLabel.Text = Lang.Get("map_gps_error");
        }
    }

    private bool HandlePreferences()
    {
        string tourPoiIds = Preferences.Get("TourPoiIds", "");
        if (!string.IsNullOrEmpty(tourPoiIds))
        {
            Preferences.Remove("TourPoiIds");
            var ids = tourPoiIds.Split(',').Select(int.Parse).ToList();
            var tourPois = _allPoisFromDb.Where(p => ids.Contains(p.PoiId)).ToList();
            if (tourPois.Any())
            {
                UpdateInterface(tourPois.First().Latitude, tourPois.First().Longitude, tourPois.First());
                StartTrackingLocation();
                return true;
            }
        }

        int highlightId = Preferences.Get("HighlightPoiId", -1);
        if (highlightId > 0)
        {
            Preferences.Remove("HighlightPoiId");
            var poi = _allPoisFromDb.FirstOrDefault(p => p.PoiId == highlightId);
            if (poi != null) UpdateInterface(poi.Latitude, poi.Longitude, poi);
            StartTrackingLocation();
            return true;
        }
        return false;
    }
    private async void OnManualSyncClicked(object sender, EventArgs e)
    {
        MapLoading.IsVisible = true;
        StatusLabel.Text = "⏳ Đang đồng bộ kịch bản mới...";

        var apiSync = new ApiSyncService(_dbService);
        bool success = await apiSync.SyncAudiosAsync();

        MapLoading.IsVisible = false;
        if (success)
        {
            await DisplayAlert("Thành công", "Đã tải kịch bản AI mới nhất!", "OK");
            _allPoisFromDb = (await _dbService.GetPOIsAsync()).ToList();
            RefreshListView();
        }
        else
        {
            await DisplayAlert("Lỗi", "Không thể kết nối với Server Web.", "Thử lại");
        }
    }
    private void LoadDefaultMap()
    {
        var html = new HtmlWebViewSource();
        html.Html = GenerateMapHtml(10.76186, 106.70224, _allPoisFromDb);
        MapWebView.Source = html;
        MapLoading.IsVisible = false;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopSpeaking();
        MapWebView.Navigating -= OnMapWebViewNavigating;
        StopTrackingLocation();
    }

    // --- SỰ KIỆN TƯƠNG TÁC (UI EVENTS) ---

    private void PlayPoiNarration(POI poi)
    {
        Task.Run(async () =>
        {
            try
            {
                // 1. LẤY KỊCH BẢN: Ưu tiên Script (TTS), fallback về DescriptionVi
                var audio = await _dbService.GetAudioByPoiIdAsync(poi.PoiId);
                string textToProcess = (audio != null && !string.IsNullOrEmpty(audio.Script))
                                       ? audio.Script
                                       : poi.DescriptionVi;

                if (string.IsNullOrEmpty(textToProcess)) return;

                // 2. DỊCH THUẬT: Chuyển ngữ nếu máy không phải Tiếng Việt
                string finalSpeech = textToProcess;
                if (!IsVietnamese(_currentLanguage))
                {
                    var translator = new TranslationService();
                    var translated = await translator.TranslateAsync(textToProcess, GetShortLangCode(_currentLanguage));
                    finalSpeech = string.IsNullOrEmpty(translated) ? textToProcess : translated;
                }

                // 3. CẬP NHẬT UI & PHÁT LOA (Phải chạy trên MainThread)
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // --- ĐÂY LÀ PHẦN PHỤC HỒI CODE CŨ CỦA BẠN ---
                    // Hiển thị lại icon tọa độ và khoảng cách trên màn hình
                    StatusLabel.Text = $"📍 {poi.DistanceDisplay}";

                    // Nếu bạn muốn đảm bảo tên quán cũng khớp thì thêm dòng này:
                    CurrentPoiName.Text = poi.Name;

                    // Phát âm thanh ra loa
                    SpeakText(finalSpeech);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== LỖI THUYẾT MINH: {ex.Message}");
            }
        });
    }

    // Cập nhật lại hàm OnMapWebViewNavigating
    private async void OnMapWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (!e.Url.StartsWith("poi://")) return;
        e.Cancel = true;

        if (!int.TryParse(e.Url.Replace("poi://", ""), out int poiId)) return;
        var poi = _allPoisFromDb.FirstOrDefault(p => p.PoiId == poiId);
        if (poi == null) return;

        MainThread.BeginInvokeOnMainThread(() => UpdateInterface(poi.Latitude, poi.Longitude, poi));

        // Gọi hàm thuyết minh
        PlayPoiNarration(poi);
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var keyword = e.NewTextValue?.ToLower() ?? "";
        if (string.IsNullOrEmpty(keyword))
        {
            SuggestionBorder.IsVisible = false;
            RefreshListView();
            return;
        }

        var suggestions = _allPoisFromDb
            .Where(p => p.Name.ToLower().Contains(keyword) || p.Category.ToLower().Contains(keyword))
            .Take(5).ToList();

        SuggestionList.ItemsSource = suggestions;
        SuggestionBorder.IsVisible = suggestions.Any();
        RefreshListView();
    }

    private async void OnSuggestionSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not POI selected) return;

        SuggestionBorder.IsVisible = false;
        SearchEntry.Text = selected.Name;
        UpdateInterface(selected.Latitude, selected.Longitude, selected);

        _ = Task.Run(async () =>
        {
            string text = selected.DescriptionVi;
            if (!IsVietnamese(_currentLanguage))
            {
                var translated = await new TranslationService().TranslateAsync(selected.DescriptionVi, GetShortLangCode(_currentLanguage));
                text = string.IsNullOrEmpty(translated) ? selected.DescriptionVi : translated;
            }
            MainThread.BeginInvokeOnMainThread(() => SpeakText(text));
        });
        SearchEntry.Unfocus();
    }

    private void OnPoiSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not POI selectedPoi) return;
        UpdateInterface(selectedPoi.Latitude, selectedPoi.Longitude, selectedPoi);
    }

    private void OnToggleFavorite(object sender, EventArgs e)
    {
        if (_currentDisplayedPoi == null) return;
        if (FavoritesPage.IsFavorite(_currentDisplayedPoi.PoiId))
            FavoritesPage.RemoveFavorite(_currentDisplayedPoi.PoiId);
        else
            FavoritesPage.AddFavorite(_currentDisplayedPoi.PoiId);

        BtnFavorite.Text = FavoritesPage.IsFavorite(_currentDisplayedPoi.PoiId) ? Lang.Get("map_favorited") : Lang.Get("map_favorite");
    }

    private async void OnNavigateClicked(object sender, EventArgs e)
    {
        if (_currentDisplayedPoi == null) return;
        try
        {
            await Map.Default.OpenAsync(new Location(_currentDisplayedPoi.Latitude, _currentDisplayedPoi.Longitude),
                new MapLaunchOptions { Name = _currentDisplayedPoi.Name, NavigationMode = NavigationMode.Walking });
        }
        catch
        {
            await Launcher.OpenAsync($"http://maps.google.com/?daddr={_currentDisplayedPoi.Latitude},{_currentDisplayedPoi.Longitude}&travelmode=walking");
        }
    }

    private void OnReplayClicked(object sender, EventArgs e)
    {
        if (_currentDisplayedPoi == null) return;

        _geofenceService.ResetPoi(_currentDisplayedPoi.PoiId);

        // Đã FIX: Không gọi sự kiện giả lập nữa mà gọi trực tiếp hàm thuyết minh
        PlayPoiNarration(_currentDisplayedPoi);
    }

    private async void OnDetailClicked(object sender, EventArgs e)
    {
        if (_currentDisplayedPoi == null) return;
        await Navigation.PushAsync(new PoiDetailPage(_currentDisplayedPoi));
    }
}