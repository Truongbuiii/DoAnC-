using FoodTourApp.Services;

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

            // Load ngôn ngữ đã lưu
            Lang.Load();

            // Sync ngầm khi mở app
            Task.Run(async () => await StartInitialSync());
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        protected override async void OnStart()
        {
            base.OnStart();

            // Chỉ hỏi ngôn ngữ nếu chưa có
            if (!Preferences.ContainsKey("AppLanguage"))
            {
                await Task.Delay(800);

                if (Shell.Current == null) return;

                string action = await Shell.Current.DisplayActionSheetAsync(
                    "Chọn ngôn ngữ thuyết minh",
                    null, null,
                    "🇻🇳 Tiếng Việt",
                    "🇺🇸 English",
                    "🇨🇳 中文",
                    "🇰🇷 한국어",
                    "🇯🇵 日本語");

                // ✅ Dùng đúng language code
                string lang = action switch
                {
                    "🇺🇸 English" => "en-US",
                    "🇨🇳 中文" => "zh-CN",
                    "🇰🇷 한국어" => "ko-KR",
                    "🇯🇵 日本語" => "ja-JP",
                    _ => "vi-VN"
                };

                Preferences.Set("AppLanguage", lang);
                Lang.Set(lang);

                // Cập nhật tab titles ngay sau khi chọn
                if (Shell.Current is AppShell appShell)
                    appShell.ApplyLanguage();
            }
        }

        protected override async void OnSleep()
        {
            base.OnSleep();
            try
            {
                await _syncService.SyncLogsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnSleep sync failed: {ex.Message}");
            }
        }

        private async Task StartInitialSync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== BẮT ĐẦU SYNC ===");
                bool poiOk = await _syncService.SyncPoisAsync();
                bool tourOk = await _syncService.SyncToursAsync();

                if (poiOk)
                    await _dbService.TranslateAndCachePoisAsync();

                await _syncService.SyncLogsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== LỖI SYNC: {ex.Message}");
            }
        }
    }
}