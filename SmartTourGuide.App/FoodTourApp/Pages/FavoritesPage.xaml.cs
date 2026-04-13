using FoodTourApp.Models;
using FoodTourApp.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;

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

        // Gọi hàm load dữ liệu
        await LoadFavoritesAsync();
    }

    private void ApplyLanguage()
    {
        Title = Lang.Get("tab_favorites");
        LblTitle.Text = Lang.Get("fav_title");
        LblEmpty.Text = Lang.Get("fav_empty");
        LblEmptySub.Text = Lang.Get("fav_empty_sub");
    }

    private async Task LoadFavoritesAsync()
    {
        var favoriteIds = GetFavoriteIds();
        var allPois = await _dbService.GetPOIsAsync();
        var favorites = allPois.Where(p => favoriteIds.Contains(p.PoiId)).ToList();

        // 1. Kiểm tra ngôn ngữ hiện tại
        string currentLang = Preferences.Get("AppLanguage", "vi-VN");
        bool isVietnamese = currentLang.StartsWith("vi", StringComparison.OrdinalIgnoreCase);

        // 2. Mớm dữ liệu tạm thời (Tránh chớp tiếng Việt)
        foreach (var p in favorites)
        {
            p.DisplayName = isVietnamese ? p.Name : "...";
            p.DisplayCategory = isVietnamese ? p.Category : "...";
        }

        // Đẩy danh sách rỗng / có dấu "..." ra màn hình trước
        FavoritesList.ItemsSource = null;
        FavoritesList.ItemsSource = favorites;

        // 3. Chạy dịch thuật ngầm nếu không phải tiếng Việt
        if (!isVietnamese && favorites.Any())
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var translator = new TranslationService();
                    string shortCode = currentLang.Split('-')[0].ToLower();

                    foreach (var p in favorites)
                    {
                        var dn = await translator.TranslateAsync(p.Name, shortCode);
                        var dc = await translator.TranslateAsync(p.Category, shortCode);

                        p.DisplayName = !string.IsNullOrEmpty(dn) ? dn : p.Name;
                        p.DisplayCategory = !string.IsNullOrEmpty(dc) ? dc : p.Category;
                    }

                    // Cập nhật lại UI sau khi dịch xong (Ép ListView vẽ lại)
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        FavoritesList.ItemsSource = null;
                        FavoritesList.ItemsSource = favorites;
                    });
                }
                catch
                {
                    // Lỗi API / Mất mạng -> Trả về lại tiếng Việt gốc
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        foreach (var p in favorites)
                        {
                            p.DisplayName = p.Name;
                            p.DisplayCategory = p.Category;
                        }
                        FavoritesList.ItemsSource = null;
                        FavoritesList.ItemsSource = favorites;
                    });
                }
            });
        }
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

        // Tải lại danh sách sau khi xóa
        await LoadFavoritesAsync();
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