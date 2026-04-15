using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using FoodTourApp.Models;

namespace FoodTourApp.Services
{
    public class ApiSyncService
    {
        private readonly DatabaseService _dbService;
        private readonly HttpClient _httpClient;

        // URL ngrok của bạn
        public const string BaseUrl = "https://tandra-acetylenic-aurelio.ngrok-free.dev";

        // CHỈ GIỮ LẠI MỘT KHAI BÁO NÀY (Đã thêm Header bỏ qua cảnh báo của Ngrok)
        private static readonly HttpClient SharedHttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(60),
            DefaultRequestHeaders = { { "ngrok-skip-browser-warning", "true" } }
        };

        public ApiSyncService(DatabaseService dbService)
        {
            _dbService = dbService;
            _httpClient = SharedHttpClient;
        }

        // 1. SYNC POI TỪ SERVER VỀ SQLITE
        public async Task<bool> SyncPoisAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{BaseUrl}/api/v1/pois");
                var pois = JsonSerializer.Deserialize<List<POI>>(response,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (pois != null && pois.Count > 0)
                {
                    foreach (var poi in pois)
                    {
                        if (!string.IsNullOrEmpty(poi.ImageSource) && !poi.ImageSource.StartsWith("http"))
                        {
                            poi.ImageSource = $"{BaseUrl}/images/{poi.ImageSource}";
                        }
                    }
                    await _dbService.SavePOIsFromServerAsync(pois);
                    Debug.WriteLine($"=== SYNC OK: {pois.Count} POIs");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"=== SYNC FAIL: {ex.Message}");
            }
            return false;
        }

        // 2. SYNC TOURS TỪ SERVER VỀ SQLITE
        public async Task<bool> SyncToursAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{BaseUrl}/api/v1/tours");
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var tours = JsonSerializer.Deserialize<List<Itinerary>>(response, options);

                if (tours != null && tours.Count > 0)
                {
                    foreach (var tour in tours)
                    {
                        if (!string.IsNullOrEmpty(tour.ImageSource) && !tour.ImageSource.StartsWith("http"))
                        {
                            tour.ImageSource = $"{BaseUrl}/images/{tour.ImageSource}";
                        }
                    }
                    await _dbService.SaveToursFromServerAsync(tours);
                    Debug.WriteLine($"=== SYNC TOURS OK: {tours.Count}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"=== SYNC TOURS FAIL: {ex.Message}");
            }
            return false;
        }

        // 3. SYNC AUDIOS (KỊCH BẢN TTS)
        public async Task<bool> SyncAudiosAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{BaseUrl}/api/v1/audios/all");
                var audios = JsonSerializer.Deserialize<List<FoodTourApp.Models.Audio>>(response,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (audios != null && audios.Count > 0)
                {
                    await _dbService.SaveAudiosFromServerAsync(audios);
                    Debug.WriteLine($"=== SYNC AUDIO OK: {audios.Count}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"=== LỖI SYNC AUDIO: {ex.Message}");
            }
            return false;
        }

        // 4. SYNC ACTIVITY LOGS LÊN SERVER
        public async Task<bool> SyncLogsAsync()
        {
            try
            {
                var unsynced = await _dbService.GetUnSyncedLogsAsync();
                if (!unsynced.Any()) return true;

                var dtos = unsynced.Select(l => new
                {
                    l.PoiId,
                    l.ActionType,
                    l.LanguageUsed,
                    l.DeviceType,
                    l.DeviceId, 
                    l.AccessTime
                }).ToList();

                var json = JsonSerializer.Serialize(dtos);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{BaseUrl}/api/v1/analytics/sync", content);

                if (response.IsSuccessStatusCode)
                {
                    var ids = unsynced.Select(l => l.LogId).ToList();
                    // Đánh dấu đã sync và xóa cục bộ để nhẹ máy
                    await _dbService.MarkLogsAsSyncedAsync(ids);
                    await _dbService.DeleteLogsByIdsAsync(ids);
                    Debug.WriteLine($"=== LOGS SYNCED AND DELETED: {unsynced.Count}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"=== LOGS FAIL: {ex.Message}");
            }
            return false;
        }
    }
}