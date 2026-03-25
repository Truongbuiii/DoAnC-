namespace FoodTourApp.Services
{
    /// <summary>
    /// LanguageService - Quản lý ngôn ngữ hiện tại của app
    /// </summary>
    public class LanguageService
    {
        // Ngôn ngữ hiện tại (mặc định tiếng Việt)
        private string _currentLanguage = "vi-VN";

        // Danh sách ngôn ngữ hỗ trợ
        public static readonly List<LanguageOption> SupportedLanguages = new()
        {
            new LanguageOption { Code = "vi-VN", Name = "Tiếng Việt", Flag = "🇻🇳" },
            new LanguageOption { Code = "en-US", Name = "English", Flag = "🇺🇸" },
            new LanguageOption { Code = "zh-CN", Name = "中文", Flag = "🇨🇳" },
            new LanguageOption { Code = "ko-KR", Name = "한국어", Flag = "🇰🇷" },
            new LanguageOption { Code = "ja-JP", Name = "日本語", Flag = "🇯🇵" }
        };

        // Event khi đổi ngôn ngữ
        public event EventHandler<string>? OnLanguageChanged;

        /// <summary>
        /// Lấy/đặt ngôn ngữ hiện tại
        /// </summary>
        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;

                    // Lưu vào Preferences để nhớ khi mở lại app
                    Preferences.Set("AppLanguage", value);

                    // Thông báo đổi ngôn ngữ
                    OnLanguageChanged?.Invoke(this, value);
                }
            }
        }

        /// <summary>
        /// Lấy thông tin ngôn ngữ hiện tại
        /// </summary>
        public LanguageOption CurrentLanguageInfo =>
            SupportedLanguages.FirstOrDefault(l => l.Code == _currentLanguage)
            ?? SupportedLanguages[0];

        /// <summary>
        /// Khởi tạo - load ngôn ngữ đã lưu
        /// </summary>
        public LanguageService()
        {
            _currentLanguage = Preferences.Get("AppLanguage", "vi-VN");
        }

        /// <summary>
        /// Lấy locale cho TTS
        /// </summary>
        public Locale GetTtsLocale()
        {
            return _currentLanguage switch
            {
                "vi-VN" => new Locale("vi", "VN"),
                "en-US" => new Locale("en", "US"),
                "zh-CN" => new Locale("zh", "CN"),
                "ko-KR" => new Locale("ko", "KR"),
                "ja-JP" => new Locale("ja", "JP"),
                _ => new Locale("vi", "VN")
            };
        }
    }

    /// <summary>
    /// Thông tin ngôn ngữ
    /// </summary>
    public class LanguageOption
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Flag { get; set; } = string.Empty;

        // Hiển thị trong Picker
        public string DisplayName => $"{Flag} {Name}";
    }

    /// <summary>
    /// Locale cho TTS (thay thế System.Globalization)
    /// </summary>
    public class Locale
    {
        public string Language { get; }
        public string Country { get; }

        public Locale(string language, string country)
        {
            Language = language;
            Country = country;
        }

        public override string ToString() => $"{Language}-{Country}";
    }
}