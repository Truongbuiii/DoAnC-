using SQLite;

namespace FoodTourApp.Models
{
    public class POI
    {
        [PrimaryKey]
        public int PoiId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageSource { get; set; } = string.Empty; // Tên file ảnh (vd: ocoanh.jpg)
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public double TriggerRadius { get; set; } // Bán kính thực tế từ database
    }
}