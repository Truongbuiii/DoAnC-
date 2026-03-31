namespace AdminWeb.Models
{
    public class Admin
    {
        // Phải khớp với tên cột AdminId trong SQL (Primary Key)
        public int AdminId { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string? FullName { get; set; }
    }
}