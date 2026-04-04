using System.Net.Http;
using System.Text;
using System.Text.Json;
using FoodTourApp.Models;

namespace FoodTourApp.Services
{
    public class ApiSyncService
    {
        private readonly DatabaseService _dbService;
        private readonly HttpClient _httpClient;

        private const string BaseUrl = "http://192.168.1.58:5110";

        public ApiSyncService(DatabaseService dbService)
        {
            _dbService = dbService;
            _httpClient = new HttpClient
            {

                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        // SYNC POI TỪ SERVER VỀ SQLITE
        public async Task<bool> SyncPoisAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{BaseUrl}/api/v1/pois");
                var pois = JsonSerializer.Deserialize<List<POI>>(response,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (pois != null && pois.Count > 0)
                {
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

        // SYNC ACTIVITY LOGS LÊN SERVER
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
                var response = await _httpClient.PostAsync(
                    $"{BaseUrl}/api/v1/analytics/sync", content);

                if (response.IsSuccessStatusCode)
                {
                    await _dbService.MarkLogsAsSyncedAsync(
                        unsynced.Select(l => l.LogId).ToList());
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
    }
}