using SQLite;

namespace FoodTourApp.Models
{
    [Table("Tours")] // Khớp với tên bảng trong SQL Server của bạn
    public class Tour
    {
        [PrimaryKey, AutoIncrement]
        public int TourId { get; set; }

        public string TourName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string TotalTime { get; set; } = string.Empty;

        public string ImageSource { get; set; } = string.Empty;
        private string? _displayName;
        [Ignore]
        public string? DisplayName
        {
            get => _displayName;
            set => _displayName = value;
        }
    }
}