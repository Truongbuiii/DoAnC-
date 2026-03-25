using FoodTourApp;
using FoodTourApp.Pages;
using FoodTourApp.Services;
using Microsoft.Extensions.Logging;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // ============================================================
        // Đăng ký các dịch vụ cốt lõi
        // ============================================================

        // Database - Singleton (dùng chung 1 instance)
        builder.Services.AddSingleton<DatabaseService>();

        // Geofence - Singleton (giữ trạng thái cooldown xuyên suốt app)
        builder.Services.AddSingleton<GeofenceService>();

        // Narration - Singleton (quản lý hàng chờ TTS)
        builder.Services.AddSingleton<NarrationService>();

        // Pages - Transient (tạo mới mỗi lần navigate)
        builder.Services.AddTransient<MapPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}