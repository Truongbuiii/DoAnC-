using SQLite;

namespace FoodTourApp.Models
{
    public class POI
    {
        [PrimaryKey, AutoIncrement]
        public int PoiId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        // ============================================================
        // MÔ TẢ ĐA NGÔN NGỮ (5 ngôn ngữ)
        // ============================================================
        public string DescriptionVi { get; set; } = string.Empty;  // 🇻🇳 Tiếng Việt
        public string DescriptionEn { get; set; } = string.Empty;  // 🇺🇸 English
        public string DescriptionZh { get; set; } = string.Empty;  // 🇨🇳 中文
        public string DescriptionKo { get; set; } = string.Empty;  // 🇰🇷 한국어
        public string DescriptionJa { get; set; } = string.Empty;  // 🇯🇵 日本語

        // Giữ lại field cũ để tương thích (mặc định = tiếng Việt)
        public string Description
        {
            get => DescriptionVi;
            set => DescriptionVi = value;
        }

        /// <summary>
        /// Lấy mô tả theo ngôn ngữ
        /// </summary>
        public string GetDescription(string languageCode)
        {
            return languageCode switch
            {
                "vi-VN" => DescriptionVi,
                "en-US" => DescriptionEn,
                "zh-CN" => DescriptionZh,
                "ko-KR" => DescriptionKo,
                "ja-JP" => DescriptionJa,
                _ => DescriptionVi // Mặc định tiếng Việt
            };
        }

        // ============================================================
        // DỮ LIỆU CHO TRANG CHI TIẾT
        // ============================================================
        public string Menu { get; set; } = string.Empty;
        public string History { get; set; } = string.Empty;

        // ============================================================
        // QUẢN LÝ ẢNH
        // ============================================================
        public string ImageSource { get; set; } = string.Empty;

        [Ignore]
        public string FullImageUrl => ImageSource;

        // ============================================================
        // GPS & VỊ TRÍ
        // ============================================================
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public double TriggerRadius { get; set; }

        // ============================================================
        // HIỂN THỊ KHOẢNG CÁCH
        // ============================================================
        [Ignore]
        public string DistanceDisplay { get; set; } = "--- m";
    }
}