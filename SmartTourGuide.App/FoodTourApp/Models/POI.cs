using SQLite;

namespace FoodTourApp.Models
{
    public class POI
    {
        [PrimaryKey, AutoIncrement]
        public int PoiId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // 1. DỮ LIỆU CHO TRANG CHI TIẾT
        public string Menu { get; set; } = string.Empty;      // Danh sách món ăn gợi ý
        public string History { get; set; } = string.Empty;   // Sự tích/Lịch sử quán

        // 2. QUẢN LÝ ẢNH (Từ Web Server)
        public string ImageSource { get; set; } = string.Empty; // Chỉ lưu tên file: "ocoanh.jpg"

        [Ignore]
        public string FullImageUrl
        {
            get
            {
                // MAUI rất thông minh, khi ảnh là MauiImage, 
                // bạn chỉ cần trả về đúng tên file (ví dụ: "ocoanh.jpg") 
                // là nó tự tìm trong Resources/Images.
                return ImageSource;
            }
        }

        // 3. GPS & VỊ TRÍ
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public double TriggerRadius { get; set; } // Bán kính kích hoạt thuyết minh (mét)

        // 4. HIỂN THỊ KHOẢNG CÁCH THỜI GIAN THỰC
        [Ignore] // Không lưu xuống DB, chỉ dùng để hiện "Cách bạn 150m" trên List
        public string DistanceDisplay { get; set; } = "--- m";
    }
}