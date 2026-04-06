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
    /// <summary>
    /// Lang - Quản lý UI labels đa ngôn ngữ
    /// </summary>
    public static class Lang
    {
        private static string _code = "vi-VN";

        public static void Load()
        {
            _code = Preferences.Get("AppLanguage", "vi-VN");
        }

        public static void Set(string code)
        {
            _code = code;
        }

        public static string Get(string key)
        {
            return _code switch
            {
                "en-US" => En.TryGetValue(key, out var en) ? en : key,
                "zh-CN" => Zh.TryGetValue(key, out var zh) ? zh : key,
                "ko-KR" => Ko.TryGetValue(key, out var ko) ? ko : key,
                "ja-JP" => Ja.TryGetValue(key, out var ja) ? ja : key,
                _ => Vi.TryGetValue(key, out var vi) ? vi : key
            };
        }

        private static readonly Dictionary<string, string> Vi = new()
        {
            ["tab_home"] = "Trang chủ",
            ["tab_map"] = "Bản đồ",
            ["tab_favorites"] = "Yêu thích",
            ["tab_qr"] = "Quét QR",
            ["tab_settings"] = "Cài đặt",
            ["home_welcome"] = "Chào mừng đến",
            ["home_title"] = "Phố ẩm thực Vĩnh Khánh 🍜",
            ["home_top10"] = "🏆 Top 10 thế giới 2025",
            ["home_explore"] = "Khám phá ngay →",
            ["home_subtitle"] = "Thiên đường ốc & hải sản Quận 4",
            ["home_featured"] = "📍 Địa điểm nổi bật",
            ["home_tours"] = "🗺️ Tour gợi ý",
            ["home_location"] = "Vĩnh Khánh, Quận 4",
            ["map_search"] = "Tìm quán ăn...",
            ["map_navigate"] = "🧭 Dẫn",
            ["map_replay"] = "🔊 Nghe lại",
            ["map_favorite"] = "🤍 Yêu thích",
            ["map_favorited"] = "❤️ Đã lưu",
            ["map_detail"] = "📋 Chi tiết",
            ["map_gps_error"] = "⚠️ Cần quyền vị trí",
            ["detail_intro"] = "Giới thiệu",
            ["detail_menu"] = "🍽️ Món nên thử",
            ["detail_favorite"] = "🤍 Yêu thích",
            ["detail_favorited"] = "❤️ Đã lưu",
            ["detail_listen"] = "🔊 Nghe",
            ["detail_map"] = "🗺️ Bản đồ",
            ["detail_navigate"] = "🧭 Dẫn đường",
            ["fav_title"] = "❤️ Địa điểm yêu thích",
            ["fav_empty"] = "Chưa có địa điểm yêu thích",
            ["fav_empty_sub"] = "Nhấn ❤️ ở bất kỳ quán nào để lưu",
            ["qr_hint"] = "Hướng camera vào mã QR của quán",
            ["qr_waiting"] = "📷 Đang chờ quét mã QR...",
            ["qr_flash"] = "🔦 Đèn flash",
            ["qr_manual"] = "📋 Nhập mã tay",
            ["qr_not_found"] = "Không tìm thấy",
            ["qr_not_found_msg"] = "Mã QR không khớp với địa điểm nào!",
            ["settings_language"] = "Ngôn ngữ",
            ["settings_lang_label"] = "Ngôn ngữ thuyết minh",
            ["settings_narration"] = "Thuyết minh",
            ["settings_auto"] = "Tự động phát",
            ["settings_auto_sub"] = "Phát khi đến gần quán",
            ["settings_gps"] = "GPS & Geofence",
            ["settings_radius"] = "Bán kính kích hoạt",
            ["settings_cooldown"] = "Thời gian chờ",
            ["settings_data"] = "Dữ liệu",
            ["settings_clear"] = "Xóa danh sách yêu thích",
            ["settings_info"] = "Thông tin",
            ["settings_version"] = "Phiên bản",
            ["settings_platform"] = "Nền tảng",
            ["btn_ok"] = "OK",
            ["btn_cancel"] = "Hủy",
            ["btn_delete"] = "Xóa",
            ["confirm_clear_fav"] = "Xóa toàn bộ danh sách yêu thích?",
            ["success_clear_fav"] = "Đã xóa danh sách yêu thích!",
            ["select_language"] = "Chọn ngôn ngữ thuyết minh",
        };

        private static readonly Dictionary<string, string> En = new()
        {
            ["tab_home"] = "Home",
            ["tab_map"] = "Map",
            ["tab_favorites"] = "Favorites",
            ["tab_qr"] = "Scan QR",
            ["tab_settings"] = "Settings",
            ["home_welcome"] = "Welcome to",
            ["home_title"] = "Vinh Khanh Food Street 🍜",
            ["home_top10"] = "🏆 Top 10 worldwide 2025",
            ["home_explore"] = "Explore now →",
            ["home_subtitle"] = "Seafood paradise in District 4",
            ["home_featured"] = "📍 Featured Places",
            ["home_tours"] = "🗺️ Suggested Tours",
            ["home_location"] = "Vinh Khanh, District 4",
            ["map_search"] = "Search restaurants...",
            ["map_navigate"] = "🧭 Navigate",
            ["map_replay"] = "🔊 Replay",
            ["map_favorite"] = "🤍 Favorite",
            ["map_favorited"] = "❤️ Saved",
            ["map_detail"] = "📋 Detail",
            ["map_gps_error"] = "⚠️ Location permission required",
            ["detail_intro"] = "Introduction",
            ["detail_menu"] = "🍽️ Must try",
            ["detail_favorite"] = "🤍 Favorite",
            ["detail_favorited"] = "❤️ Saved",
            ["detail_listen"] = "🔊 Listen",
            ["detail_map"] = "🗺️ Map",
            ["detail_navigate"] = "🧭 Navigate",
            ["fav_title"] = "❤️ Favorite Places",
            ["fav_empty"] = "No favorite places yet",
            ["fav_empty_sub"] = "Tap ❤️ on any restaurant to save",
            ["qr_hint"] = "Point camera at restaurant QR code",
            ["qr_waiting"] = "📷 Waiting for QR scan...",
            ["qr_flash"] = "🔦 Flash",
            ["qr_manual"] = "📋 Enter code",
            ["qr_not_found"] = "Not found",
            ["qr_not_found_msg"] = "QR code doesn't match any location!",
            ["settings_language"] = "Language",
            ["settings_lang_label"] = "Narration language",
            ["settings_narration"] = "Narration",
            ["settings_auto"] = "Auto play",
            ["settings_auto_sub"] = "Play when near a restaurant",
            ["settings_gps"] = "GPS & Geofence",
            ["settings_radius"] = "Trigger radius",
            ["settings_cooldown"] = "Wait time",
            ["settings_data"] = "Data",
            ["settings_clear"] = "Clear favorites",
            ["settings_info"] = "Information",
            ["settings_version"] = "Version",
            ["settings_platform"] = "Platform",
            ["btn_ok"] = "OK",
            ["btn_cancel"] = "Cancel",
            ["btn_delete"] = "Delete",
            ["confirm_clear_fav"] = "Clear all favorite places?",
            ["success_clear_fav"] = "Favorites cleared!",
            ["select_language"] = "Select narration language",
        };

        private static readonly Dictionary<string, string> Zh = new()
        {
            ["tab_home"] = "首页",
            ["tab_map"] = "地图",
            ["tab_favorites"] = "收藏",
            ["tab_qr"] = "扫码",
            ["tab_settings"] = "设置",
            ["home_welcome"] = "欢迎来到",
            ["home_title"] = "永康美食街 🍜",
            ["home_top10"] = "🏆 2025年全球前10",
            ["home_explore"] = "立即探索 →",
            ["home_subtitle"] = "第四郡海鲜天堂",
            ["home_featured"] = "📍 热门地点",
            ["home_tours"] = "🗺️ 推荐路线",
            ["home_location"] = "永康, 第四郡",
            ["map_search"] = "搜索餐厅...",
            ["map_navigate"] = "🧭 导航",
            ["map_replay"] = "🔊 重播",
            ["map_favorite"] = "🤍 收藏",
            ["map_favorited"] = "❤️ 已收藏",
            ["map_detail"] = "📋 详情",
            ["map_gps_error"] = "⚠️ 需要位置权限",
            ["detail_intro"] = "介绍",
            ["detail_menu"] = "🍽️ 必点菜",
            ["detail_favorite"] = "🤍 收藏",
            ["detail_favorited"] = "❤️ 已收藏",
            ["detail_listen"] = "🔊 收听",
            ["detail_map"] = "🗺️ 地图",
            ["detail_navigate"] = "🧭 导航",
            ["fav_title"] = "❤️ 收藏地点",
            ["fav_empty"] = "暂无收藏地点",
            ["fav_empty_sub"] = "点击 ❤️ 收藏任意餐厅",
            ["qr_hint"] = "将相机对准餐厅二维码",
            ["qr_waiting"] = "📷 等待扫描二维码...",
            ["qr_flash"] = "🔦 闪光灯",
            ["qr_manual"] = "📋 手动输入",
            ["qr_not_found"] = "未找到",
            ["qr_not_found_msg"] = "二维码与任何地点不匹配！",
            ["settings_language"] = "语言",
            ["settings_lang_label"] = "讲解语言",
            ["settings_narration"] = "讲解",
            ["settings_auto"] = "自动播放",
            ["settings_auto_sub"] = "靠近餐厅时播放",
            ["settings_gps"] = "GPS与地理围栏",
            ["settings_radius"] = "触发半径",
            ["settings_cooldown"] = "等待时间",
            ["settings_data"] = "数据",
            ["settings_clear"] = "清除收藏",
            ["settings_info"] = "信息",
            ["settings_version"] = "版本",
            ["settings_platform"] = "平台",
            ["btn_ok"] = "确定",
            ["btn_cancel"] = "取消",
            ["btn_delete"] = "删除",
            ["confirm_clear_fav"] = "清除所有收藏地点？",
            ["success_clear_fav"] = "收藏已清除！",
            ["select_language"] = "选择讲解语言",
        };

        private static readonly Dictionary<string, string> Ko = new()
        {
            ["tab_home"] = "홈",
            ["tab_map"] = "지도",
            ["tab_favorites"] = "즐겨찾기",
            ["tab_qr"] = "QR 스캔",
            ["tab_settings"] = "설정",
            ["home_welcome"] = "에 오신 것을 환영합니다",
            ["home_title"] = "빈칸 푸드 스트리트 🍜",
            ["home_top10"] = "🏆 2025 세계 Top 10",
            ["home_explore"] = "지금 탐험하기 →",
            ["home_subtitle"] = "4군 해산물 천국",
            ["home_featured"] = "📍 인기 장소",
            ["home_tours"] = "🗺️ 추천 투어",
            ["home_location"] = "빈칸, 4군",
            ["map_search"] = "식당 검색...",
            ["map_navigate"] = "🧭 안내",
            ["map_replay"] = "🔊 다시 듣기",
            ["map_favorite"] = "🤍 즐겨찾기",
            ["map_favorited"] = "❤️ 저장됨",
            ["map_detail"] = "📋 상세",
            ["map_gps_error"] = "⚠️ 위치 권한 필요",
            ["detail_intro"] = "소개",
            ["detail_menu"] = "🍽️ 추천 메뉴",
            ["detail_favorite"] = "🤍 즐겨찾기",
            ["detail_favorited"] = "❤️ 저장됨",
            ["detail_listen"] = "🔊 듣기",
            ["detail_map"] = "🗺️ 지도",
            ["detail_navigate"] = "🧭 길 안내",
            ["fav_title"] = "❤️ 즐겨찾기 장소",
            ["fav_empty"] = "즐겨찾기 장소 없음",
            ["fav_empty_sub"] = "식당의 ❤️ 를 눌러 저장하세요",
            ["qr_hint"] = "식당 QR 코드에 카메라를 향하세요",
            ["qr_waiting"] = "📷 QR 스캔 대기 중...",
            ["qr_flash"] = "🔦 플래시",
            ["qr_manual"] = "📋 코드 입력",
            ["qr_not_found"] = "찾을 수 없음",
            ["qr_not_found_msg"] = "QR 코드가 장소와 일치하지 않습니다!",
            ["settings_language"] = "언어",
            ["settings_lang_label"] = "해설 언어",
            ["settings_narration"] = "해설",
            ["settings_auto"] = "자동 재생",
            ["settings_auto_sub"] = "식당 근처에서 재생",
            ["settings_gps"] = "GPS & 지오펜스",
            ["settings_radius"] = "활성화 반경",
            ["settings_cooldown"] = "대기 시간",
            ["settings_data"] = "데이터",
            ["settings_clear"] = "즐겨찾기 지우기",
            ["settings_info"] = "정보",
            ["settings_version"] = "버전",
            ["settings_platform"] = "플랫폼",
            ["btn_ok"] = "확인",
            ["btn_cancel"] = "취소",
            ["btn_delete"] = "삭제",
            ["confirm_clear_fav"] = "즐겨찾기를 모두 지우시겠습니까?",
            ["success_clear_fav"] = "즐겨찾기가 지워졌습니다!",
            ["select_language"] = "해설 언어 선택",
        };

        private static readonly Dictionary<string, string> Ja = new()
        {
            ["tab_home"] = "ホーム",
            ["tab_map"] = "地図",
            ["tab_favorites"] = "お気に入り",
            ["tab_qr"] = "QRスキャン",
            ["tab_settings"] = "設定",
            ["home_welcome"] = "へようこそ",
            ["home_title"] = "ヴィンカン・フードストリート 🍜",
            ["home_top10"] = "🏆 2025年世界トップ10",
            ["home_explore"] = "今すぐ探索 →",
            ["home_subtitle"] = "4区のシーフードの楽園",
            ["home_featured"] = "📍 人気スポット",
            ["home_tours"] = "🗺️ おすすめツアー",
            ["home_location"] = "ヴィンカン, 4区",
            ["map_search"] = "レストランを検索...",
            ["map_navigate"] = "🧭 案内",
            ["map_replay"] = "🔊 再生",
            ["map_favorite"] = "🤍 お気に入り",
            ["map_favorited"] = "❤️ 保存済み",
            ["map_detail"] = "📋 詳細",
            ["map_gps_error"] = "⚠️ 位置情報の許可が必要",
            ["detail_intro"] = "紹介",
            ["detail_menu"] = "🍽️ おすすめメニュー",
            ["detail_favorite"] = "🤍 お気に入り",
            ["detail_favorited"] = "❤️ 保存済み",
            ["detail_listen"] = "🔊 聴く",
            ["detail_map"] = "🗺️ 地図",
            ["detail_navigate"] = "🧭 ナビ",
            ["fav_title"] = "❤️ お気に入りの場所",
            ["fav_empty"] = "お気に入りの場所がありません",
            ["fav_empty_sub"] = "レストランの ❤️ を押して保存",
            ["qr_hint"] = "レストランのQRコードにカメラを向けてください",
            ["qr_waiting"] = "📷 QRスキャン待機中...",
            ["qr_flash"] = "🔦 フラッシュ",
            ["qr_manual"] = "📋 コード入力",
            ["qr_not_found"] = "見つかりません",
            ["qr_not_found_msg"] = "QRコードがどの場所とも一致しません！",
            ["settings_language"] = "言語",
            ["settings_lang_label"] = "解説言語",
            ["settings_narration"] = "解説",
            ["settings_auto"] = "自動再生",
            ["settings_auto_sub"] = "レストラン近くで再生",
            ["settings_gps"] = "GPS & ジオフェンス",
            ["settings_radius"] = "起動半径",
            ["settings_cooldown"] = "待機時間",
            ["settings_data"] = "データ",
            ["settings_clear"] = "お気に入りを削除",
            ["settings_info"] = "情報",
            ["settings_version"] = "バージョン",
            ["settings_platform"] = "プラットフォーム",
            ["btn_ok"] = "OK",
            ["btn_cancel"] = "キャンセル",
            ["btn_delete"] = "削除",
            ["confirm_clear_fav"] = "お気に入りをすべて削除しますか？",
            ["success_clear_fav"] = "お気に入りを削除しました！",
            ["select_language"] = "解説言語を選択",
        };
    }
}