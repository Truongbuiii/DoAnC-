using SQLite;

namespace FoodTourApp.Models
{
    public class Itinerary
    {
        [PrimaryKey]
        public int TourId { get; set; }

        public string TourName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TotalTime { get; set; } = "60 phút";
        public string ImageSource { get; set; } = string.Empty;

        // Lưu danh sách PoiId dạng "1,2,3" — không có trong web nhưng cần cho app
        public string PoiIdsRaw { get; set; } = string.Empty;

        [Ignore]
        public string Name => TourName; // alias để XAML binding vẫn dùng được

        [Ignore]
        public string Duration => TotalTime; // alias cho XAML

        private string? _displayName;
        [Ignore]
        public string? DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
            }
        }

        [Ignore]
        public List<int> PoiIds
        {
            get
            {
                if (string.IsNullOrEmpty(PoiIdsRaw)) return new List<int>();
                return PoiIdsRaw.Split(',').Select(int.Parse).ToList();
            }
            set
            {
                PoiIdsRaw = string.Join(",", value);
            }
        }
    }
}