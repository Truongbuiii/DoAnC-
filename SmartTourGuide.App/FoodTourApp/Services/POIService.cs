using FoodTourApp.Models;

namespace FoodTourApp.Services;

public class POIService
{
    public List<POI> GetPOIs()
    {
        return new List<POI>
        {
            new POI
            {
                Name = "Bánh mì Huỳnh Hoa",
                Description = "Bánh mì nổi tiếng Sài Gòn",
                Latitude = 10.7725,
                Longitude = 106.6917
            },

            new POI
            {
                Name = "Phở Hòa",
                Description = "Phở truyền thống",
                Latitude = 10.7867,
                Longitude = 106.6905
            }
        };
    }
}