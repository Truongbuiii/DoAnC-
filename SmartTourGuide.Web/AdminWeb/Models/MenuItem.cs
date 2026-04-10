using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminWeb.Models
{
    [Table("MenuItems")]
    public class MenuItem
    {
        [Key]
        [Column("MenuId")]
        public int MenuId { get; set; }

        public int PoiId { get; set; }

        public string? DishName { get; set; }

        public string? Price { get; set; }

        public string? ImageSource { get; set; }

        public bool IsRecommended { get; set; }
    }
}
