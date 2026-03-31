using FoodTourApp.Models;

#if ANDROID
using FoodTourApp.Platforms.Android;
#endif

namespace FoodTourApp.Pages;

public partial class PoiDetailPage : ContentPage
{
    private readonly POI _poi;
    private string _currentLanguage;

#if ANDROID
    private AndroidTtsService? _androidTts;
#endif

    public PoiDetailPage(POI poi)
    {
        InitializeComponent();
        _poi = poi;
        _currentLanguage = Preferences.Get("AppLanguage", "vi-VN");

        // Set BindingContext để XAML binding hoạt động như MainPage
        BindingContext = _poi;

        if (_poi != null)
        {
            DetailName.Text = _poi.Name;
            DetailCategory.Text = _poi.Category.ToUpper();
            DetailDescription.Text = _poi.GetDescription(_currentLanguage);
        }

        UpdateFavoriteButton();
    }

    private void LoadImage()
    {
        if (string.IsNullOrEmpty(_poi?.ImageSource)) return;
        try
        {
            // Dùng string trực tiếp như XAML binding — không dùng ImageSource.FromFile()
            MainImage.Source = _poi.ImageSource;
        }
        catch { }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
#if ANDROID
        _androidTts = new AndroidTtsService();
        await _androidTts.InitializeAsync();
#endif
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
#if ANDROID
        _androidTts?.Stop();
#endif
    }

    private void UpdateFavoriteButton()
    {
        if (_poi == null) return;
        BtnFavorite.Text = FavoritesPage.IsFavorite(_poi.PoiId)
            ? "❤️ Đã lưu"
            : "🤍 Yêu thích";
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Navigation.PopAsync();

    private void OnFavoriteClicked(object sender, EventArgs e)
    {
        if (_poi == null) return;
        if (FavoritesPage.IsFavorite(_poi.PoiId))
            FavoritesPage.RemoveFavorite(_poi.PoiId);
        else
            FavoritesPage.AddFavorite(_poi.PoiId);
        UpdateFavoriteButton();
    }

    private void OnSpeakClicked(object sender, EventArgs e)
    {
        if (_poi == null) return;
        string text = _poi.GetDescription(_currentLanguage);
#if ANDROID
        _androidTts?.SetLanguage(_currentLanguage);
        _androidTts?.Speak(text);
#endif
    }

    private async void OnDirectClicked(object sender, EventArgs e)
    {
        if (_poi == null) return;
        try
        {
            var location = new Location(_poi.Latitude, _poi.Longitude);
            var options = new MapLaunchOptions { Name = _poi.Name, NavigationMode = NavigationMode.Walking };
            await Map.Default.OpenAsync(location, options);
        }
        catch
        {
            string lat = _poi.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string lon = _poi.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            await Launcher.OpenAsync($"https://www.google.com/maps/dir/?api=1&destination={lat},{lon}&travelmode=walking");
        }
    }

    private async void OnViewOnMapClicked(object sender, EventArgs e)
    {
        if (_poi == null) return;
        Preferences.Set("HighlightPoiId", _poi.PoiId);
        await Shell.Current.GoToAsync("//MapPage");
    }
}