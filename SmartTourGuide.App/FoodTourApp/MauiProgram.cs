using FoodTourApp;
using FoodTourApp.Pages;
using FoodTourApp.Services;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui.Controls;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .UseBarcodeReader() // ← thêm dòng này
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // ============================================================
        // Đăng ký các dịch vụ cốt lõi
        // ============================================================

        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<GeofenceService>();
        builder.Services.AddSingleton<NarrationService>();
        builder.Services.AddSingleton<TranslationService>();
        builder.Services.AddSingleton<ApiSyncService>();
        builder.Services.AddTransient<MapPage>();
        builder.Services.AddSingleton<App>();
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}