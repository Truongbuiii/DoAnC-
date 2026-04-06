using FoodTourApp.Services;

namespace FoodTourApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Lang.Load(); // load trước
        ApplyLanguage(); // apply ngay
    }

    public void ApplyLanguage()
    {
        TabHome.Title = Lang.Get("tab_home");
        TabFavorites.Title = Lang.Get("tab_favorites");
        TabMap.Title = Lang.Get("tab_map");
        TabQR.Title = Lang.Get("tab_qr");
        TabSettings.Title = Lang.Get("tab_settings");
    }
}