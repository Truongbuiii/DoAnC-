using SQLite;

namespace FoodTourApp.Models
{
    [Table("MenuItems")] // THÊM DÒNG NÀY: Để nó khớp với tên bảng trong SQL script
    public class MenuItemModel
    {
        [PrimaryKey, AutoIncrement]
        public int MenuId { get; set; }
        public int PoiId { get; set; }
        public string DishName { get; set; }
        public string Price { get; set; }
        public string ImageSource { get; set; }
        public bool IsRecommended { get; set; }
    }
}