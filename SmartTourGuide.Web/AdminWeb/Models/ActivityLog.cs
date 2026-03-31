using System.ComponentModel.DataAnnotations;

namespace AdminWeb.Models
{
    public class ActivityLogs
    {
        [Key]
        public int LogId { get; set; }
        public int PoiId { get; set; }
        public string? ActionType { get; set; }
        public string? LanguageUsed { get; set; }
        public string? DeviceType { get; set; }
        public DateTime AccessTime { get; set; }

        public virtual POI? Poi { get; set; }
    }
}