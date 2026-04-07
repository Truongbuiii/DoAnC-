using FoodTourApp.Models;
using Microsoft.Maui.Devices.Sensors;

namespace FoodTourApp.Services
{
    /// <summary>
    /// GeofenceService - Phát hiện POI trong bán kính kích hoạt
    /// Có debounce và cooldown để tránh spam thuyết minh
    /// </summary>
    public class GeofenceService
    {
        // Danh sách POI đã phát trong phiên này (tránh lặp)
        private readonly HashSet<int> _playedPoiIds = new();
        
        // Thời gian cooldown cho mỗi POI (phút)
        private readonly Dictionary<int, DateTime> _poiCooldowns = new();
        
        // Cooldown mặc định: 5 phút (không phát lại POI đã nghe trong 5 phút)
        public int CooldownMinutes { get; set; } = 5;
        
        // Debounce: Phải ở trong vùng ít nhất X giây mới trigger
        public int DebounceSeconds { get; set; } = 3;

        // Thêm property DefaultRadius
        public int DefaultRadius { get; set; } = 0; // 0 = dùng radius của từng POI

        // Theo dõi thời gian vào vùng
        private readonly Dictionary<int, DateTime> _enterTimes = new();

        /// <summary>
        /// Kiểm tra và trả về POI cần thuyết minh (nếu có)
        /// </summary>
        /// <param name="userLat">Vĩ độ người dùng</param>
        /// <param name="userLon">Kinh độ người dùng</param>
        /// <param name="allPois">Danh sách tất cả POI</param>
        /// <returns>POI cần phát thuyết minh, hoặc null nếu không có</returns>
        public GeofenceResult CheckGeofences(double userLat, double userLon, List<POI> allPois)
        {
            var userLocation = new Location(userLat, userLon);
            var result = new GeofenceResult();
            
            POI nearestPoi = null;
            double nearestDistance = double.MaxValue;

            foreach (var poi in allPois)
            {
                // Tính khoảng cách
                double distKm = Location.CalculateDistance(
                    userLocation, 
                    poi.Latitude, 
                    poi.Longitude, 
                    DistanceUnits.Kilometers
                );
                double distMeters = distKm * 1000;
                
                // Cập nhật khoảng cách hiển thị
                poi.DistanceDisplay = distMeters < 1000 
                    ? $"{distMeters:F0} m" 
                    : $"{distKm:F1} km";

                // Kiểm tra có trong bán kính không
                double radius = DefaultRadius > 0 ? DefaultRadius : poi.TriggerRadius;
                if (distMeters <= radius)
                {
                    // Tìm POI gần nhất trong vùng
                    if (distMeters < nearestDistance)
                    {
                        nearestDistance = distMeters;
                        nearestPoi = poi;
                    }
                }
                else
                {
                    // Ra khỏi vùng -> xóa thời gian vào
                    _enterTimes.Remove(poi.PoiId);
                }
            }

            result.NearestPoi = nearestPoi;
            result.DistanceMeters = nearestDistance;

            // Nếu có POI trong vùng
            if (nearestPoi != null)
            {
                // Kiểm tra debounce
                if (!_enterTimes.ContainsKey(nearestPoi.PoiId))
                {
                    _enterTimes[nearestPoi.PoiId] = DateTime.Now;
                }

                var timeInZone = (DateTime.Now - _enterTimes[nearestPoi.PoiId]).TotalSeconds;
                
                // Phải ở trong vùng đủ lâu (debounce)
                if (timeInZone >= DebounceSeconds)
                {
                    // Kiểm tra cooldown
                    if (CanTriggerPoi(nearestPoi.PoiId))
                    {
                        result.ShouldNarrate = true;
                        result.PoiToNarrate = nearestPoi;
                        
                        // Đánh dấu đã phát
                        MarkAsPlayed(nearestPoi.PoiId);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Kiểm tra POI có thể trigger không (đã qua cooldown chưa)
        /// </summary>
        private bool CanTriggerPoi(int poiId)
        {
            // Chưa từng phát -> OK
            if (!_poiCooldowns.ContainsKey(poiId))
                return true;

            // Kiểm tra đã qua cooldown chưa
            var lastPlayed = _poiCooldowns[poiId];
            var elapsed = (DateTime.Now - lastPlayed).TotalMinutes;
            
            return elapsed >= CooldownMinutes;
        }

        /// <summary>
        /// Đánh dấu POI đã được phát
        /// </summary>
        private void MarkAsPlayed(int poiId)
        {
            _playedPoiIds.Add(poiId);
            _poiCooldowns[poiId] = DateTime.Now;
        }

        /// <summary>
        /// Reset tất cả (khi người dùng muốn nghe lại từ đầu)
        /// </summary>
        public void ResetAll()
        {
            _playedPoiIds.Clear(); 
            _poiCooldowns.Clear();
            _enterTimes.Clear();
        }

        /// <summary>
        /// Reset cooldown cho 1 POI cụ thể (cho phép nghe lại)
        /// </summary>
        public void ResetPoi(int poiId)
        {
            _playedPoiIds.Remove(poiId);
            _poiCooldowns.Remove(poiId);
            _enterTimes.Remove(poiId);
        }

        /// <summary>
        /// Lấy danh sách POI đã phát
        /// </summary>
        public List<int> GetPlayedPoiIds()
        {
            return _playedPoiIds.ToList();
        }
    }

    /// <summary>
    /// Kết quả kiểm tra Geofence
    /// </summary>
    public class GeofenceResult
    {
        public POI? NearestPoi { get; set; }
        public double DistanceMeters { get; set; }
        public bool ShouldNarrate { get; set; }
        public POI? PoiToNarrate { get; set; }
    }
}
