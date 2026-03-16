using System.ComponentModel.DataAnnotations;

namespace AdminWeb.Models
{
    public class Location
    {
        [Key]
        public int LocationId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
        public double TriggerRadius { get; set; }
    }
}