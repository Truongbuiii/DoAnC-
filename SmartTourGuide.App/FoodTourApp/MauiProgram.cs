using FoodTourApp; // QUAN TRỌNG: Phải có dòng này để trình biên dịch tìm thấy lớp App
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
            // .UseMauiMaps() // Hãy thêm dấu // vào đây vì chúng ta đã dùng WebView để tránh lỗi Key trên Windows
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Đăng ký các dịch vụ cốt lõi
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddTransient<MapPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}