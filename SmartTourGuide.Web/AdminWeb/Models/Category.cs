using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // 👈 Thêm dòng này để dùng [Table] và [Column]

namespace AdminWeb.Models
{
    [Table("Categories")] // 👈 Chỉ định rõ tên bảng trong SQL
    public class Category
    {
        [Key]
        [Column("CategoryId")] // 👈 Ép máy tìm đúng cột CategoryId trong SQL
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        public string CategoryName { get; set; } = string.Empty;
    }
}