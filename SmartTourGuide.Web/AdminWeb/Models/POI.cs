using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace AdminWeb.Models
{
    [Table("POIs")]
    public class POI
    {
        [Key]
        [Column("PoiId")]
        public int PoiId { get; set; }

        [Required(ErrorMessage = "Tên quán không được để trống")]
        public string Name { get; set; } = string.Empty;

        public string? Category { get; set; } // Giờ là kiểu chuỗi (NVARCHAR)
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double TriggerRadius { get; set; }
        public string? ImageSource { get; set; }

        // --- 5 CỘT ĐA NGÔN NGỮ ---
        public string? DescriptionVi { get; set; }
        public string? DescriptionEn { get; set; }
        public string? DescriptionZh { get; set; }
        public string? DescriptionKo { get; set; }
        public string? DescriptionJa { get; set; }

        // Kết nối với bảng Audio (Quan hệ 1 - Nhiều)
        public virtual ICollection<Audio>? Audios { get; set; }
    }
}