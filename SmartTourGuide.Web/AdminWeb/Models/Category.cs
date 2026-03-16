using System.ComponentModel.DataAnnotations;

namespace AdminWeb.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        [Required]
        public string CategoryName { get; set; } = string.Empty;
    }
}