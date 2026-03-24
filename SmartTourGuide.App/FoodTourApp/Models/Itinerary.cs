using SQLite;
using System.Text.Json; // Cần thiết nếu bạn muốn dùng Json

namespace FoodTourApp.Models
{
    public class Itinerary
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Duration { get; set; } = "60 phút";
        public int StopCount { get; set; }

        // 1. QUẢN LÝ ẢNH (Giống bên POI)
        public string ImageSource { get; set; } = string.Empty; // Lưu: "tour_oc.jpg"

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

        // 2. MẸO LƯU DANH SÁCH ID VÀO SQLITE
        // Cột này sẽ lưu trong DB dưới dạng: "1,2,5,10"
        public string PoiIdsRaw { get; set; } = string.Empty;

        [Ignore] // Thuộc tính này dùng để code logic trong App, không lưu xuống DB
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