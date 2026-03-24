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

    public MapPage()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
        _ttsCancelSource = new CancellationTokenSource();
        PoisListView.ItemsSource = _poiListForDisplay;
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

        // BƯỚC SỬA QUAN TRỌNG: Link HTTPS chuẩn Google Maps (hl=vi để hiện tiếng Việt)
        string googleMapsUrl = $"https://www.google.com/maps?q={lat},{lon}&hl=vi";

        if (poi != null)
        {
            PoiCard.IsVisible = true;
            CurrentPoiName.Text = poi.Name;
            CategoryLabel.Text = poi.Category.ToUpper();
            PoiImage.Source = poi.FullImageUrl;
            StatusLabel.Text = "Bạn đang ở gần: " + poi.Name;

            // Chỉ load lại Map khi đổi địa điểm để máy không lag
            MapWebView.Source = googleMapsUrl;
        }
        else
        {
            StatusLabel.Text = $"📍 GPS: {lat:F5}, {lon:F5}";
            if (MapWebView.Source == null) MapWebView.Source = googleMapsUrl;
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
            UpdateInterface(selectedPoi.Latitude, selectedPoi.Longitude, selectedPoi);
            await SpeakDescription(selectedPoi.Description);
        }
    }

    private async void SimulateOcOanh(object sender, EventArgs e) => await CheckAndNarrate(10.75883, 106.70505);
    private async void SimulateOcVu(object sender, EventArgs e) => await CheckAndNarrate(10.75916, 106.70452);

    private async void SimulateReset(object sender, EventArgs e)
    {
        _lastPlayedPoi = string.Empty;
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