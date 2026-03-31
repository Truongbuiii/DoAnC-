using Microsoft.Extensions.DependencyInjection;

namespace FoodTourApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            base.OnStart();

            // Chỉ hỏi lần đầu tiên mở app
            bool hasChosenLanguage = Preferences.ContainsKey("AppLanguage");
            if (!hasChosenLanguage)
            {
                await Task.Delay(500); // chờ UI load xong

                string action = await Shell.Current.DisplayActionSheet(
                    "Chọn ngôn ngữ thuyết minh",
                    null, null,
                    "🇻🇳 Tiếng Việt",
                    "🇺🇸 English",
                    "🇨🇳 中文",
                    "🇰🇷 한국어",
                    "🇯🇵 日本語");

                string lang = action switch
                {
                    "🇺🇸 English" => "en-US",
                    "🇨🇳 中文" => "zh-CN",
                    "🇰🇷 한국어" => "ko-KR",
                    "🇯🇵 日本語" => "ja-JP",
                    _ => "vi-VN"
                };

                Preferences.Set("AppLanguage", lang);
            }
        }
    }
}