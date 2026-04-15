using SQLite;

namespace FoodTourApp.Models
{
    public class ActivityLog
    {
        [PrimaryKey, AutoIncrement]
        public int LogId { get; set; }
        public int PoiId { get; set; }
        public string ActionType { get; set; } = string.Empty; // AutoTrigger / ScanQR
        public string LanguageUsed { get; set; } = string.Empty;
        public string DeviceType { get; set; } = "Android";
public string? DeviceId { get; set; }
        public DateTime AccessTime { get; set; } = DateTime.Now;
        public int IsSynced { get; set; } = 0; // 0: chưa sync, 1: đã sync
    }
}