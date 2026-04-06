using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminWeb.Models
{
    public class Tour
    {
        [Key]
        public int TourId { get; set; }

        [Required(ErrorMessage = "Tên Tour không được để trống")]
        [Display(Name = "Tên Hành Trình")]
        public string TourName { get; set; } = null!;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Thời gian dự kiến")]
        public string? TotalTime { get; set; }

        // --- BỔ SUNG THUỘC TÍNH NÀY ---
        [Display(Name = "Hình ảnh đại diện")]
        public string? ImageSource { get; set; }

        // Nối sang bảng chi tiết để lấy danh sách quán ốc
        public virtual ICollection<TourDetail> TourDetails { get; set; } = new List<TourDetail>();
    }
}