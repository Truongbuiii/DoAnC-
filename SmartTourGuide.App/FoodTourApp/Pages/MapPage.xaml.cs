using System.Collections.ObjectModel;
using FoodTourApp.Models;
using FoodTourApp.Services;
using Microsoft.Maui.Devices.Sensors;

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

        _geofenceService.CooldownMinutes = 1;
        _geofenceService.DebounceSeconds = 1;

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

    private void ApplyLanguage()
    {
        Title = Lang.Get("tab_map");
        LblMapTitle.Text = $"📍 {Lang.Get("map_title")}";
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

    private string GetLanguageFlag(string code) => code switch
    {
        "vi-VN" => "🇻🇳",
        "en-US" => "🇺🇸",
        "zh-CN" => "🇨🇳",
        "ko-KR" => "🇰🇷",
        "ja-JP" => "🇯🇵",
        _ => "🌐"
    };

    private string GenerateMapHtml(double centerLat, double centerLon, List<POI> allPois, int highlightPoiId = -1)
    {
        var markersJs = string.Join("\n", allPois.Select(p =>
        {
            string lat = p.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string lon = p.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string name = p.Name.Replace("'", "\\'");
            string cat = p.Category.Replace("'", "\\'");

            bool isHighlight = p.PoiId == highlightPoiId;
            string iconColor = isHighlight ? "orange" : "green";
            string iconSize = isHighlight ? "40" : "30";

            return $@"
        var icon_{p.PoiId} = L.divIcon({{
            className: '',
            html: '<div style=""width:{iconSize}px;height:{iconSize}px;background:{iconColor};border-radius:50% 50% 50% 0;transform:rotate(-45deg);border:3px solid white;box-shadow:0 2px 5px rgba(0,0,0,0.3)""></div>',
            iconSize: [{iconSize},{iconSize}],
            iconAnchor: [{int.Parse(iconSize) / 2},{int.Parse(iconSize)}]
        }});
        var marker_{p.PoiId} = L.marker([{lat},{lon}], {{icon: icon_{p.PoiId}}})
            .bindPopup('<b>{name}</b><br/><i>{cat}</i>{(isHighlight ? "<br/>📍 Gần nhất!" : "")}')
            .on('click', function() {{ window.location.href = 'poi://{p.PoiId}'; }});
        {(isHighlight ? $"marker_{p.PoiId}.openPopup();" : "")}
        markerCluster.addLayer(marker_{p.PoiId});";
        }));

        string latStr = centerLat.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string lonStr = centerLon.ToString(System.Globalization.CultureInfo.InvariantCulture);

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'/>
    <link rel='stylesheet' href='https://unpkg.com/leaflet.markercluster@1.5.3/dist/MarkerCluster.css'/>
    <link rel='stylesheet' href='https://unpkg.com/leaflet.markercluster@1.5.3/dist/MarkerCluster.Default.css'/>
    <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
    <script src='https://unpkg.com/leaflet.markercluster@1.5.3/dist/leaflet.markercluster.js'></script>
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

        var markerCluster = L.markerClusterGroup({{
            maxClusterRadius: 50,
            disableClusteringAtZoom: 18,
            spiderfyOnMaxZoom: true
        }});

        {markersJs}
        markerCluster.addTo(map);

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
                }}).addTo(map).bindPopup('📍 You');

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

    private async Task DrawBlueDotAfterLoad(double lat, double lon)
    {
        await Task.Delay(800);
        string latStr = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string lonStr = lon.ToString(System.Globalization.CultureInfo.InvariantCulture);
        MainThread.BeginInvokeOnMainThread(() =>
            MapWebView.Eval($"setUserLocation({latStr}, {lonStr}, 15)"));
    }

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
    private async void OnLocationUpdated(object? sender, Microsoft.Maui.Devices.Sensors.Location location)
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
            string latStr = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string lonStr = lon.ToString(System.Globalization.CultureInfo.InvariantCulture);
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
                SpeakText(result.PoiToNarrate.GetDescription(_currentLanguage));
                _ = _dbService.LogActivityAsync(
                    result.PoiToNarrate.PoiId, "AutoTrigger", _currentLanguage);
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

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        Lang.Load();
        _currentLanguage = Preferences.Get("AppLanguage", "vi-VN");
        ApplyLanguage();

        var apiSync = new ApiSyncService(_dbService);
        _ = Task.Run(async () =>
        {
            var syncResult = await apiSync.SyncPoisAsync();
            System.Diagnostics.Debug.WriteLine($"=== SYNC POI: {syncResult}");
            var logResult = await apiSync.SyncLogsAsync();
            System.Diagnostics.Debug.WriteLine($"=== SYNC LOGS: {logResult}");
        });

        _geofenceService.CooldownMinutes = Preferences.Get("CooldownMinutes", 5);
        _geofenceService.DefaultRadius = Preferences.Get("TriggerRadius", 0);

#if ANDROID
        _androidTts = new AndroidTtsService();
        await _androidTts.InitializeAsync();
#endif

        MapWebView.Navigating += OnMapWebViewNavigating;

        var pois = await _dbService.GetPOIsAsync();
        _allPoisFromDb = pois.ToList();
        RefreshListView();

        string tourPoiIds = Preferences.Get("TourPoiIds", "");
        if (!string.IsNullOrEmpty(tourPoiIds))
        {
            Preferences.Remove("TourPoiIds");
            var ids = tourPoiIds.Split(',').Select(int.Parse).ToList();
            var tourPois = _allPoisFromDb.Where(p => ids.Contains(p.PoiId)).ToList();
            if (tourPois.Any())
            {
                var html = new HtmlWebViewSource();
                html.Html = GenerateMapHtml(tourPois.First().Latitude, tourPois.First().Longitude, tourPois);
                MapWebView.Source = html;
                MapLoading.IsVisible = false;
                StartTrackingLocation();
                return;
            }
        }

        int highlightId = Preferences.Get("HighlightPoiId", -1);
        if (highlightId > 0)
        {
            Preferences.Remove("HighlightPoiId");
            var poi = _allPoisFromDb.FirstOrDefault(p => p.PoiId == highlightId);
            if (poi != null) UpdateInterface(poi.Latitude, poi.Longitude, poi);
            StartTrackingLocation();
            return;
        }

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
                    var html = new HtmlWebViewSource();
                    html.Html = GenerateMapHtml(location.Latitude, location.Longitude, _allPoisFromDb);
                    MapWebView.Source = html;
                    MapLoading.IsVisible = false;
                    await Task.Delay(1000);
                    string latStr = location.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    string lonStr = location.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    MapWebView.Eval($"setUserLocation({latStr}, {lonStr}, 15)");
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
    }

    private void OnMapWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (!e.Url.StartsWith("poi://")) return;
        e.Cancel = true;

        if (!int.TryParse(e.Url.Replace("poi://", ""), out int poiId)) return;
        var poi = _allPoisFromDb.FirstOrDefault(p => p.PoiId == poiId);
        if (poi == null) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            _currentDisplayedPoi = poi;
            PoiCard.IsVisible = true;
            CurrentPoiName.Text = poi.Name;
            CategoryLabel.Text = poi.Category.ToUpper();
            PoiImage.Source = poi.FullImageUrl;
            StatusLabel.Text = Lang.Get("map_selected");
            BtnFavorite.Text = FavoritesPage.IsFavorite(poi.PoiId)
                ? Lang.Get("map_favorited") : Lang.Get("map_favorite");
            MapLoading.IsVisible = false;
            SpeakText(poi.GetDescription(_currentLanguage));
        });
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var keyword = e.NewTextValue?.ToLower() ?? "";
        if (string.IsNullOrEmpty(keyword))
        {
            SuggestionBorder.IsVisible = false;
            SuggestionList.ItemsSource = null;
            RefreshListView();
            return;
        }

        var suggestions = _allPoisFromDb
            .Where(p => p.Name.ToLower().Contains(keyword) ||
                        p.Category.ToLower().Contains(keyword))
            .Take(5).ToList();

        if (suggestions.Any())
        {
            SuggestionList.ItemsSource = suggestions;
            SuggestionBorder.IsVisible = true;
        }
        else
        {
            SuggestionBorder.IsVisible = false;
        }

        RefreshListView();
    }

    private void OnSuggestionSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not POI selected) return;

        SuggestionBorder.IsVisible = false;
        SuggestionList.SelectedItem = null;
        SearchEntry.Text = selected.Name;

        _currentDisplayedPoi = selected;
        PoiCard.IsVisible = true;
        CurrentPoiName.Text = selected.Name;
        CategoryLabel.Text = selected.Category.ToUpper();
        PoiImage.Source = selected.FullImageUrl;
        StatusLabel.Text = "🔍";
        BtnFavorite.Text = FavoritesPage.IsFavorite(selected.PoiId)
            ? Lang.Get("map_favorited") : Lang.Get("map_favorite");

        string latStr = selected.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string lonStr = selected.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        MapWebView.Eval($"map.setView([{latStr},{lonStr}], 18)");

        SpeakText(selected.GetDescription(_currentLanguage));
        SearchEntry.Unfocus();
    }

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
            BtnFavorite.Text = Lang.Get("map_favorite");
        }
        else
        {
            FavoritesPage.AddFavorite(_currentDisplayedPoi.PoiId);
            BtnFavorite.Text = Lang.Get("map_favorited");
        }
    }

    private async void OnNavigateClicked(object sender, EventArgs e)
    {
        if (_currentDisplayedPoi == null)
        {
            await DisplayAlertAsync(Lang.Get("btn_ok"), "Chưa có địa điểm nào được chọn!", Lang.Get("btn_ok"));
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
            await DisplayAlertAsync(Lang.Get("btn_ok"), "Chưa có địa điểm nào được chọn!", Lang.Get("btn_ok"));
            return;
        }
        _geofenceService.ResetPoi(_currentDisplayedPoi.PoiId);
        SpeakText(_currentDisplayedPoi.GetDescription(_currentLanguage));
    }

    private async void OnDetailClicked(object sender, EventArgs e)
    {
        if (_currentDisplayedPoi == null) return;
        await Navigation.PushAsync(new PoiDetailPage(_currentDisplayedPoi));
    }
}