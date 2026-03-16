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
                Name = "Ốc Oanh",
                Description = "Quán ốc nổi tiếng nhất phố Vĩnh Khánh, nổi vị muối ớt và chân gà nướng.",
                Latitude = 10.7602,
                Longitude = 106.7025
            },
            new POI
            {
                Name = "Súp Cua Hạnh",
                Description = "Súp cua nóng hổi với đầy đủ topping, cực kỳ đông khách vào buổi tối.",
                Latitude = 10.7610,
                Longitude = 106.7018
            },
            new POI
            {
                Name = "Ốc Vũ",
                Description = "Không gian rộng rãi, thực đơn đa dạng các món hải sản tươi sống.",
                Latitude = 10.7605,
                Longitude = 106.7030
            }
        };
    }
}