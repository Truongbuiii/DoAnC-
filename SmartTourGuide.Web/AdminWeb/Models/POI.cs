namespace AdminWeb.Models
{
    public class POI
    {
        public int PoiId { get; set; }

        public string Name { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double TriggerRadius { get; set; }

        public string? AudioFileName { get; set; }

        public string? ImageSource { get; set; }

        public string? Description { get; set; }

        public int CategoryId { get; set; }
    }
}