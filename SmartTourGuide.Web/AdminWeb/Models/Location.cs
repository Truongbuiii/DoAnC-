using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // 👈 Thêm dòng này để dùng được [Column]

namespace AdminWeb.Models
{
    [Table("Locations")] // 👈 Đảm bảo trỏ đúng bảng Locations trong SQL
    public class Location
    {
        [Key]
        [Column("LocationId")] // 👈 Chỉ định rõ tên cột là LocationId (khớp 100% với SQL)
        public int LocationId { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
        public double TriggerRadius { get; set; }
    }
}