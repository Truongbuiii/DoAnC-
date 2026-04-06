using FoodTourApp.Models;
using FoodTourApp.Services;

namespace FoodTourApp.Pages;

public partial class FavoritesPage : ContentPage
{
    private readonly DatabaseService _dbService;

    public FavoritesPage()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Lang.Load();
        ApplyLanguage();
        await LoadFavorites();
    }

    private void ApplyLanguage()
    {
        Title = Lang.Get("tab_favorites");
        LblTitle.Text = Lang.Get("fav_title");
        LblEmpty.Text = Lang.Get("fav_empty");
        LblEmptySub.Text = Lang.Get("fav_empty_sub");
    }

    private async Task LoadFavorites()
    {
        var favoriteIds = GetFavoriteIds();
        var allPois = await _dbService.GetPOIsAsync();
        var favorites = allPois.Where(p => favoriteIds.Contains(p.PoiId)).ToList();
        FavoritesList.ItemsSource = favorites;
    }

    private async void OnPoiSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not POI selected) return;
        FavoritesList.SelectedItem = null;
        await Navigation.PushAsync(new PoiDetailPage(selected));
    }

    private async void OnRemoveFavorite(object sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.CommandParameter is not int poiId) return;

        bool confirm = await DisplayAlertAsync(
            Lang.Get("tab_favorites"),
            Lang.Get("confirm_clear_fav"),
            Lang.Get("btn_delete"),
            Lang.Get("btn_cancel"));

        if (!confirm) return;
        RemoveFavorite(poiId);
        await LoadFavorites();
    }

    public static List<int> GetFavoriteIds()
    {
        var raw = Preferences.Get("favorites", "");
        if (string.IsNullOrEmpty(raw)) return new List<int>();
        return raw.Split(',').Select(int.Parse).ToList();
    }

    public static void AddFavorite(int poiId)
    {
        var ids = GetFavoriteIds();
        if (!ids.Contains(poiId))
        {
            ids.Add(poiId);
            Preferences.Set("favorites", string.Join(',', ids));
        }
    }

    public static void RemoveFavorite(int poiId)
    {
        var ids = GetFavoriteIds();
        ids.Remove(poiId);
        Preferences.Set("favorites", string.Join(',', ids));
    }

    public static bool IsFavorite(int poiId)
        => GetFavoriteIds().Contains(poiId);
}