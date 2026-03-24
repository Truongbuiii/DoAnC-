using Microsoft.Maui.Devices.Sensors;

namespace FoodTourApp.Services
{
    public static class LocationHelper
    {
        // Hàm tính khoảng cách giữa 2 tọa độ (trả về mét)
        public static double CalculateDistance(double userLat, double userLon, double poiLat, double poiLon)
        {
            // Tọa độ người dùng
            Location userLocation = new Location(userLat, userLon);
            // Tọa độ quán ăn
            Location poiLocation = new Location(poiLat, poiLon);

            // Dùng hàm có sẵn của MAUI để tính (đơn vị dặm hoặc km)
            double distanceInKm = Location.CalculateDistance(userLocation, poiLocation, DistanceUnits.Kilometers);

            // Đổi sang Mét
            return distanceInKm * 1000;
        }
    }
}