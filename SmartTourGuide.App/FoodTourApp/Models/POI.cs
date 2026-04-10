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

    }
}