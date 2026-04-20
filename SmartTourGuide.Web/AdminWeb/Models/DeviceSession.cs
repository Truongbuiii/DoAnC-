using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminWeb.Models
{
    [Table("DeviceSessions")]
    public class DeviceSession
    {
        [Key]
        public int SessionId { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string? DeviceName { get; set; }
        public DateTime LastSeen { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }
}