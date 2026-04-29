using FoodTourApp.Models;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel; // Thêm thư viện này để thao tác MainThread

namespace FoodTourApp.Services
{
    public class GeofenceService
    {
        private readonly HashSet<int> _playedPoiIds = new();
        private readonly Dictionary<int, DateTime> _poiCooldowns = new();

        public int CooldownMinutes { get; set; } = 5;
        public int DebounceSeconds { get; set; } = 3;
        public int DefaultRadius { get; set; } = 0;

        private readonly Dictionary<int, DateTime> _enterTimes = new();
        private int _roundRobinIndex = 0;

        /// <summary>
        /// TỐI ƯU HÓA: Chạy ngầm (Task.Run) để không làm giật giao diện
        /// </summary>
        public async Task<GeofenceResult> CheckGeofencesAsync(double userLat, double userLon, List<POI> cachedPois)
        {
            // Đẩy toàn bộ vòng lặp tính toán nặng nề xuống Background Thread
            return await Task.Run(() =>
            {
                var userLocation = new Location(userLat, userLon);
                var result = new GeofenceResult();

                POI? nearestPoi = null;
                double nearestDistance = double.MaxValue;
                var poisInRange = new List<(POI poi, double distance)>();

                foreach (var poi in cachedPois)
                {
                    // Tính khoảng cách Haversine
                    double distKm = Location.CalculateDistance(
                        userLocation,
                        poi.Latitude,
                        poi.Longitude,
                        DistanceUnits.Kilometers
                    );
                    double distMeters = distKm * 1000;

                    // BẮT BUỘC: Cập nhật giao diện (thuộc tính hiển thị) phải đẩy về MainThread
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        poi.DistanceDisplay = distMeters < 1000
                            ? $"{distMeters:F0} m"
                            : $"{distKm:F1} km";
                    });

                    // Kiểm tra có trong bán kính không
                    double radius = DefaultRadius > 0 ? DefaultRadius : poi.TriggerRadius;
                    if (distMeters <= radius)
                    {
                        poisInRange.Add((poi, distMeters));
                        if (distMeters < nearestDistance)
                        {
                            nearestDistance = distMeters;
                            nearestPoi = poi;
                        }
                    }
                    else
                    {
                        _enterTimes.Remove(poi.PoiId);
                    }
                }

                //Điều kiện để xét đọc POI nào trước khi có cùng trong khu vực
    var poisInRangeSorted = poisInRange
        // 1. Khoảng cách: Gần nhất ăn ưu tiên tuyệt đối
        .OrderBy(x => x.distance) 
        
        // 2. Chế độ Tour: Đang đi tour Michelin thì chỉ tập trung quán Michelin
        .ThenByDescending(x => tourPoiIds != null && tourPoiIds.Contains(x.poi.PoiId))
        
        // 3. Sở thích cá nhân: Ngang k/c thì tiện thể đọc luôn quán đã thả tim
        .ThenByDescending(x => FavoritesPage.IsFavorite(x.poi.PoiId))
        
        // 4. Quán có mô tả dài ưu tiên đọc trước quán có mô tả ngắn
        .ThenByDescending(x => x.poi.DescriptionVi?.Length ?? 0) 
        // Quán có mô tả 500 ký tự sẽ được ưu tiên đọc trước quán chỉ có 50 ký tự
        
        // 5. Chốt sổ khách quan: Nếu vẫn hòa mọi thứ, ưu tiên quán to hơn (bán kính rộng hơn)
        .ThenByDescending(x => x.poi.TriggerRadius)
        
        // 6. Tên POI: Nếu vẫn chưa phân định được, xếp theo thứ tự ABC tên POI
        .ThenBy(x => x.poi.Name)
        
        // (Dự phòng cho tương lai: Nếu thêm cột Rating thì ThenByDescending(x => x.poi.Rating))
        
        .Select(x => x.poi)
        .ToList();
                result.NearestPoi = nearestPoi;
                result.DistanceMeters = nearestDistance;

                if (poisInRangeSorted.Count > 0)
                {
                    // Lấy POI theo vòng tròn
                    var poiToNarrate = poisInRangeSorted[_roundRobinIndex % poisInRangeSorted.Count];

                    if (!_enterTimes.ContainsKey(poiToNarrate.PoiId))
                        _enterTimes[poiToNarrate.PoiId] = DateTime.Now;

                    var timeInZone = (DateTime.Now - _enterTimes[poiToNarrate.PoiId]).TotalSeconds;

                    result.NearestPoi = poisInRangeSorted[0]; // Hiện card POI gần nhất
                    result.DistanceMeters = nearestDistance;

                    if (timeInZone >= DebounceSeconds && CanTriggerPoi(poiToNarrate.PoiId))
                    {
                        result.ShouldNarrate = true;
                        result.PoiToNarrate = poiToNarrate;
                        MarkAsPlayed(poiToNarrate.PoiId);
                        _roundRobinIndex++;
                    }
                }

                return result;
            });
        }

        private bool CanTriggerPoi(int poiId)
        {
            if (!_poiCooldowns.ContainsKey(poiId))
                return true;

            var lastPlayed = _poiCooldowns[poiId];
            var elapsed = (DateTime.Now - lastPlayed).TotalMinutes;

            return elapsed >= CooldownMinutes;
        }

        private void MarkAsPlayed(int poiId)
        {
            _playedPoiIds.Add(poiId);
            _poiCooldowns[poiId] = DateTime.Now;
        }

        public void ResetAll()
        {
            _playedPoiIds.Clear();
            _poiCooldowns.Clear();
            _enterTimes.Clear();
        }

        public void ResetPoi(int poiId)
        {
            _playedPoiIds.Remove(poiId);
            _poiCooldowns.Remove(poiId);
            _enterTimes.Remove(poiId);
        }

        public List<int> GetPlayedPoiIds()
        {
            return _playedPoiIds.ToList();
        }
    }

    public class GeofenceResult
    {
        public POI? NearestPoi { get; set; }
        public double DistanceMeters { get; set; }
        public bool ShouldNarrate { get; set; }
        public POI? PoiToNarrate { get; set; }
    }
}