using FoodTourApp.Services;
using Microsoft.Extensions.DependencyInjection;
using FoodTourApp.Models;

namespace FoodTourApp
{
    public partial class App : Application
    {
        private readonly ApiSyncService _syncService;
        private readonly DatabaseService _dbService;

        public App(ApiSyncService syncService, DatabaseService dbService)
        {
            InitializeComponent();

            _syncService = syncService;
            _dbService = dbService;

            // Load ngôn ngữ đã chọn từ Preferences
            // Lang.Load(); 

            // 2. Kích hoạt đồng bộ ngầm ngay khi mở App (Dùng Task.Run để không chặn UI)
            Task.Run(async () => await StartInitialSync());
        }

        protected override async void OnSleep()
        {
            base.OnSleep();
            try
            {
                var apiSync = new ApiSyncService(new DatabaseService());
                await apiSync.SyncLogsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnSleep sync failed: {ex.Message}");
            }
        }

        // FIX CS0618: Di chuyển CreateWindow ra ngoài Constructor
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        private async Task StartInitialSync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== BẮT ĐẦU ĐỒNG BỘ TỪ WEB SERVER ===");
                bool poiOk = await _syncService.SyncPoisAsync();
                bool tourOk = await _syncService.SyncToursAsync();

                if (poiOk)
                {
                    await _dbService.TranslateAndCachePoisAsync();
                }

                await _syncService.SyncLogsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== LỖI KHỞI TẠO DỮ LIỆU: {ex.Message}");
            }
        }

        protected override async void OnStart()
        {
            base.OnStart();

            bool hasChosenLanguage = Preferences.ContainsKey("AppLanguage");
            if (!hasChosenLanguage)
            {
                // Phải đợi một chút để Shell.Current không bị null
                await Task.Delay(1000);

                if (Shell.Current != null)
                {
                    // FIX CS0618: Dùng DisplayActionSheetAsync thay vì DisplayActionSheet
                    string action = await Shell.Current.DisplayActionSheetAsync(
                        "Chọn ngôn ngữ thuyết minh",
                        null, null,
                        "🇻🇳 Tiếng Việt",
                        "🇺🇸 English",
                        "🇨🇳 中文",
                        "🇰🇷 한국어",
                        "🇯🇵 日本語");

                    string lang = action switch
                    {
                        "🇺🇸 English" => "en",
                        "🇨🇳 中文" => "zh",
                        "🇰🇷 한국어" => "ko",
                        "🇯🇵 日本語" => "ja",
                        _ => "vi"
                    };

                    Preferences.Set("AppLanguage", lang);
                }
            }
        }
    }
}