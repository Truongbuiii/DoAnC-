using System.Net.Http;
using System.Text;
using System.Text.Json; // Dùng thư viện này cho đồng bộ
using System.Diagnostics; // Để sửa lỗi 'Debug'
using FoodTourApp.Models;

namespace FoodTourApp.Services
{
    public class ApiSyncService
    {
        private readonly DatabaseService _dbService;
        private readonly HttpClient _httpClient;

        // URL ngrok của bạn
        public const string BaseUrl = "https://tandra-acetylenic-aurelio.ngrok-free.dev";

        public ApiSyncService(DatabaseService dbService)
        {
            _dbService = dbService;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(60)
            };
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
                    System.Diagnostics.Debug.WriteLine($"=== SYNC OK: {pois.Count} POIs");
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== SYNC FAIL: {ex.Message}");
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
                    System.Diagnostics.Debug.WriteLine($"=== SYNC TOURS OK: {tours.Count}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== SYNC TOURS FAIL: {ex.Message}");
            }
            return false;
        }

        // 3. SYNC AUDIOS (KỊCH BẢN TTS) - ĐÃ FIX LỖI
        public async Task<bool> SyncAudiosAsync()
        {
            try
            {
                // ĐÃ FIX: Thêm BaseUrl vào phía trước
                var response = await _httpClient.GetStringAsync($"{BaseUrl}/api/v1/audios/all");

                // ĐÃ FIX: Chuyển sang dùng JsonSerializer (System.Text.Json) cho đồng bộ với cả file
                var audios = JsonSerializer.Deserialize<List<FoodTourApp.Models.Audio>>(response,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (audios != null && audios.Count > 0)
                {
                    await _dbService.SaveAudiosFromServerAsync(audios);
                    System.Diagnostics.Debug.WriteLine($"=== SYNC AUDIO OK: {audios.Count}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                // ĐÃ FIX: Dùng System.Diagnostics.Debug để tránh lỗi CS0103
                System.Diagnostics.Debug.WriteLine($"=== LỖI SYNC AUDIO: {ex.Message}");
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
                    l.AccessTime
                }).ToList();

                var json = JsonSerializer.Serialize(dtos);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{BaseUrl}/api/v1/analytics/sync", content);

                if (response.IsSuccessStatusCode)
                {
                    await _dbService.MarkLogsAsSyncedAsync(unsynced.Select(l => l.LogId).ToList());
                    System.Diagnostics.Debug.WriteLine($"=== LOGS SYNCED: {unsynced.Count}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== LOGS FAIL: {ex.Message}");
            }
            return false;
        }

        // 5. SYNC MENU ITEMS TỪ SERVER VỀ SQLITE
        public async Task<bool> SyncMenuItemsAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{BaseUrl}/api/v1/menuitems");
                var items = JsonSerializer.Deserialize<List<MenuItemModel>>(response,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (items != null && items.Count > 0)
                {
                    foreach (var it in items)
                    {
                        if (!string.IsNullOrEmpty(it.ImageSource) && !it.ImageSource.StartsWith("http"))
                        {
                            it.ImageSource = $"{BaseUrl}/images/{it.ImageSource}";
                        }
                    }
                    await _dbService.SaveMenuItemsFromServerAsync(items);
                    System.Diagnostics.Debug.WriteLine($"=== SYNC MENU OK: {items.Count} items");
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== SYNC MENU FAIL: {ex.Message}");
            }
            return false;
        }
    }
}