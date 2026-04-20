using System.Text;
using System.Text.Json;
using System.Diagnostics;

namespace FoodTourApp.Services
{
    public class HeartbeatService
    {
        private readonly HttpClient _httpClient;
        private System.Threading.Timer? _timer;
        private string _deviceId = "";
        private string _deviceName = "";

        public HeartbeatService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10),
                DefaultRequestHeaders = { { "ngrok-skip-browser-warning", "true" } }
            };
        }

        public void Start()
        {
            _deviceId = Preferences.Get("DeviceUniqueId", "");
            if (string.IsNullOrEmpty(_deviceId))
            {
                _deviceId = DeviceInfo.Current.Name + "_" + Guid.NewGuid().ToString("N")[..8];
                Preferences.Set("DeviceUniqueId", _deviceId);
            }
            _deviceName = DeviceInfo.Current.Name;

            _ = SendHeartbeat();

            _timer?.Dispose();
            _timer = new System.Threading.Timer(async _ =>
            {
                await SendHeartbeat();
            }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

            Debug.WriteLine($"=== HEARTBEAT STARTED: {_deviceId}");
        }

        public void Stop()
        {
            _timer?.Dispose();
            _timer = null;
            _ = SendOffline();
            Debug.WriteLine($"=== HEARTBEAT STOPPED: {_deviceId}");
        }

        private async Task SendHeartbeat()
        {
            try
            {
                var dto = new { DeviceId = _deviceId, DeviceName = _deviceName };
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await _httpClient.PostAsync(
                    $"{ApiSyncService.BaseUrl}/api/v1/heartbeat", content);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"=== HEARTBEAT FAIL: {ex.Message}");
            }
        }

        private async Task SendOffline()
        {
            try
            {
                var dto = new { DeviceId = _deviceId, DeviceName = _deviceName };
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await _httpClient.PostAsync(
                    $"{ApiSyncService.BaseUrl}/api/v1/heartbeat/offline", content);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"=== OFFLINE FAIL: {ex.Message}");
            }
        }
    }
}