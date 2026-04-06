using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminWeb.Models
{
    [Table("ActivityLogs")]
    public class ActivityLog
    {
        [Key]
        public int LogId { get; set; }

        public int? PoiId { get; set; }

        [ForeignKey("PoiId")]
        public virtual POI? Poi { get; set; }

        public string? ActionType { get; set; } // Ví dụ: 'Listen'
        public string? LanguageUsed { get; set; } // Ví dụ: 'VI', 'EN'
        public string? DeviceType { get; set; } // Ví dụ: 'iPhone', 'Android'
        public DateTime? AccessTime { get; set; }
    }
}