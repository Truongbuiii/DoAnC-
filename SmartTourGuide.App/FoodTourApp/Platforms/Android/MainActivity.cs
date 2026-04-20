using Android.App;
using Android.Content; // Thêm cái này nếu chưa có
using Android.Content.PM;
using Android.OS;

namespace FoodTourApp
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]

    // Sửa lại ở đây: Thêm "Android.Content." vào trước các chữ Intent
    [IntentFilter(new[] { Android.Content.Intent.ActionView },
              Categories = new[] { Android.Content.Intent.CategoryDefault, Android.Content.Intent.CategoryBrowsable },
              DataScheme = "vinhkhanh",
              DataHost = "start-tour")]

    public class MainActivity : MauiAppCompatActivity
    {
    }
}