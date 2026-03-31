using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminWeb.Models
{
    [Table("Audios")]
    public class Audio
    {
        [Key]
        [Column("AudioId")]
        public int AudioId { get; set; }

        [Required(ErrorMessage = "Tên âm thanh không được để trống")]
        public string AudioName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? FilePath { get; set; }

        public string? Language { get; set; }

        // Khóa ngoại trỏ về bảng POIs
        public int PoiId { get; set; }

        [ForeignKey("PoiId")]
        public virtual POI? Poi { get; set; }
    }
}