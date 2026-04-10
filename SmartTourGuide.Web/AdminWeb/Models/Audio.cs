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

        // Text content for Text-to-Speech (the script to be read)
        public string? Script { get; set; }

        // Optional: stored audio file path when an actual audio file is uploaded
        public string? AudioFilePath { get; set; }

        // NOTE: Previous column `FilePath` existed in DB schema. Keep property only if needed
        // public string? FilePath { get; set; }

        public string? Language { get; set; }

        // Khóa ngoại trỏ về bảng POIs
        public int PoiId { get; set; }

        [ForeignKey("PoiId")]
        public virtual POI? Poi { get; set; }
    }
}