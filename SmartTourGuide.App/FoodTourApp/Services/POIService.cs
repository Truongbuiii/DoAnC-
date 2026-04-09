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
                Latitude = 10.7602,
                Longitude = 106.7025
            },
            new POI
            {
                Name = "Súp Cua Hạnh",
                Latitude = 10.7610,
                Longitude = 106.7018
            },
            new POI
            {
                Name = "Ốc Vũ",
                Latitude = 10.7605,
                Longitude = 106.7030
            }
        };
    }
}