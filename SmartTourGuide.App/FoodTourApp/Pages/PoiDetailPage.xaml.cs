using FoodTourApp.Models;

namespace FoodTourApp.Pages;

public partial class PoiDetailPage : ContentPage
{
    private readonly POI _poi;

    // QUAN TRỌNG: Constructor phải nhận vào đối tượng POI
    public PoiDetailPage(POI poi)
    {
        InitializeComponent();
        _poi = poi;

        // Gán dữ liệu lên giao diện
        if (_poi != null)
        {
            MainImage.Source = _poi.ImageSource;
            DetailName.Text = _poi.Name;
            DetailCategory.Text = _poi.Category.ToUpper();
            DetailDescription.Text = _poi.Description;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnFavoriteClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Thông báo", $"Đã thêm {_poi.Name} vào danh sách yêu thích!", "OK");
    }

    private async void OnDirectClicked(object sender, EventArgs e)
    {
        string url = $"https://www.google.com/maps/search/?api=1&query={_poi.Latitude},{_poi.Longitude}";
        await Launcher.OpenAsync(url);
    }
}