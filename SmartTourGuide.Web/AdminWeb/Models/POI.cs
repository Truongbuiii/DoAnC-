using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminWeb.Models
{
    [Table("POIs")] // Chỉ định chính xác tên bảng trong SQL của Tài
    public class POI
    {
        [Key] // Khai báo đây là Khóa chính
        public int PoiId { get; set; }

        [Required(ErrorMessage = "Tên quán không được để trống")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageSource { get; set; }

        // --- KHÓA NGOẠI CATEGORY (1-n) ---
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        // --- KHÓA NGOẠI LOCATION (1-1) ---
        public int LocationId { get; set; }

        [ForeignKey("LocationId")]
        public virtual Location? Location { get; set; }
    }
}