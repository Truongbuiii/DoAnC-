using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminWeb.Models
{
    public class TourDetail
    {
        [Key]
        public int TourDetailId { get; set; }

        public int TourId { get; set; }
        public int PoiId { get; set; }

        [Display(Name = "Thứ tự")]
        public int Order { get; set; }

        [ForeignKey("TourId")]
        public virtual Tour? Tour { get; set; }

        [ForeignKey("PoiId")]
        public virtual POI? Poi { get; set; }
    }
}