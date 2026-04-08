using FoodTourApp.Services;
using SQLite;

namespace FoodTourApp.Models
{
    public class POI
    {
        [PrimaryKey]
        public int PoiId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double TriggerRadius { get; set; }
        public string ImageSource { get; set; } = string.Empty;

        // CHỈ GIỮ TIẾNG VIỆT
        public string DescriptionVi { get; set; } = string.Empty;

        // CACHE CÁC NGÔN NGỮ ĐÃ DỊCH
        public string? DescriptionEn { get; set; }
        public string? DescriptionZh { get; set; }
        public string? DescriptionKo { get; set; }
        public string? DescriptionJa { get; set; }

        // MENU MÓN NÊN THỬ
        public string? Menu { get; set; }
        [Ignore]
        public string FullImageUrl => string.IsNullOrEmpty(ImageSource)
            ? ""
            : ImageSource.StartsWith("http")
                ? ImageSource
                : $"{ApiSyncService.BaseUrl}/images/{ImageSource}";

        [Ignore]
        public string DistanceDisplay { get; set; } = string.Empty;

        public string GetDescription(string langCode) => langCode.ToLower() switch
        {
            "en" or "en-us" => DescriptionEn ?? DescriptionVi,
            "zh" or "zh-cn" => DescriptionZh ?? DescriptionVi,
            "ko" or "ko-kr" => DescriptionKo ?? DescriptionVi,
            "ja" or "ja-jp" => DescriptionJa ?? DescriptionVi,
            _ => DescriptionVi
        };
    }
}