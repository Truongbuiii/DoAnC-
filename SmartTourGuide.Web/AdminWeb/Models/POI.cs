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

        public string? Category { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double TriggerRadius { get; set; } = 100;
        public string? ImageSource { get; set; }
        public string? OwnerUsername { get; set; }
        public string Status { get; set; } = "Hoạt động"; // Mặc định là Hoạt động khi tạo mới

        // BỔ SUNG CÁC CỘT NÀY ĐỂ HẾT LỖI CS1061
        public string? DescriptionVi { get; set; }

        public virtual ICollection<Audio>? Audios { get; set; }
    }
}