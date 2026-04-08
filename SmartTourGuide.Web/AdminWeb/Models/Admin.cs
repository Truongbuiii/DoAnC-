using System.ComponentModel.DataAnnotations; // Phải có dòng này để dùng [Required]

namespace AdminWeb.Models
{
    public class Admin
    {
        [Key]
        public int AdminId { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public string Role { get; set; } = "Owner";
    }
}