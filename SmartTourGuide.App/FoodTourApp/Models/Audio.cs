using SQLite;

namespace FoodTourApp.Models
{
    public class Audio
    {
        [PrimaryKey]
        public int AudioId { get; set; }

        public string? AudioName { get; set; }

        public string? Description { get; set; }

        // Đây là cột quan trọng nhất để AI đọc ra loa
        public string? Script { get; set; }

        public string? AudioFilePath { get; set; }

        [Indexed]
        public int PoiId { get; set; }
    }
}