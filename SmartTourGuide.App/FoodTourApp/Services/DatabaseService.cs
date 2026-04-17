using FoodTourApp.Models;
using SQLite;
using System.Diagnostics;

// Tuyệt đối KHÔNG dùng 'using static Android.Provider.MediaStore' ở đây.

namespace FoodTourApp.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;

        // ==========================================
        // 1. KHỞI TẠO DATABASE (MIGRATION)
        // ==========================================
        private async Task Init()
        {
            if (_database is not null) return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "VinhKhanhTour.db3");
            _database = new SQLiteAsyncConnection(dbPath);

            // Đăng ký đầy đủ các bảng của bạn
            await _database.CreateTableAsync<POI>();
            await _database.CreateTableAsync<Itinerary>();
            await _database.CreateTableAsync<Tour>();
            await _database.CreateTableAsync<ActivityLog>();

            // QUAN TRỌNG: Tạo bảng Audio (dùng tên đầy đủ để tránh trùng MediaStore)
            await _database.CreateTableAsync<FoodTourApp.Models.Audio>();
        }

        // ==========================================
        // 2. LOGIC DỊCH THUẬT & AUDIO (TTS)
        // ==========================================
        public async Task TranslateAndCachePoisAsync()
        {
            await Init();
            Debug.WriteLine("=== TranslateAndCachePoisAsync: no-op with current API (only DescriptionVi) ===");
        }

        // Đã sửa lỗi trùng tên Audio của Android
        public async Task<FoodTourApp.Models.Audio?> GetAudioByPoiIdAsync(int poiId)
        {
            await Init();
            // Logic 1-1: Chỉ lấy đúng 1 kịch bản gắn với PoiId này
            return await _database!.Table<FoodTourApp.Models.Audio>()
                                   .FirstOrDefaultAsync(a => a.PoiId == poiId);
        }

        // ==========================================
        // 3. ĐỒNG BỘ DỮ LIỆU TỪ SERVER
        // ==========================================
        public async Task SavePOIsFromServerAsync(List<POI> pois)
        {
            await Init();
            var serverIds = pois.Select(p => p.PoiId).ToHashSet();
            var localPois = await _database!.Table<POI>().ToListAsync();
            foreach (var local in localPois)
            {
                if (!serverIds.Contains(local.PoiId))
                    await _database.DeleteAsync(local);
            }
            foreach (var poi in pois)
                await _database!.InsertOrReplaceAsync(poi);
            Debug.WriteLine($"=== SYNC OK: {pois.Count} POIs");
        }

        public async Task SaveToursFromServerAsync(List<Itinerary> tours)
        {
            await Init();
            var serverIds = tours.Select(t => t.TourId).ToHashSet();
            var localTours = await _database!.Table<Itinerary>().ToListAsync();
            foreach (var local in localTours)
            {
                if (!serverIds.Contains(local.TourId))
                    await _database.DeleteAsync(local);
            }
            foreach (var tour in tours)
                await _database!.InsertOrReplaceAsync(tour);
            Debug.WriteLine($"=== SYNC TOURS OK: {tours.Count}");
        }

        public async Task SaveAudiosFromServerAsync(List<FoodTourApp.Models.Audio> audios)
        {
            await Init();
            if (audios == null) return;
            foreach (var audio in audios)
                await _database!.InsertOrReplaceAsync(audio);
            Debug.WriteLine($"=== ĐÃ ĐỒNG BỘ {audios.Count} KỊCH BẢN ÂM THANH ===");
        }

        // ==========================================
        // 4. QUẢN LÝ NHẬT KÝ (LOGS)
        // ==========================================
        public async Task LogActivityAsync(int poiId, string actionType, string language)
        {
            await Init();
            var log = new ActivityLog
            {
                PoiId = poiId,
                ActionType = actionType,
                LanguageUsed = language,
                DeviceType = DeviceInfo.Platform.ToString(),
DeviceId = DeviceInfo.Current.Name + "_" + DeviceInfo.Platform.ToString(), // thêm
                AccessTime = DateTime.Now,
                IsSynced = 0
            };
            await _database!.InsertAsync(log);
        }

        public async Task MarkLogsAsSyncedAsync(List<int> logIds)
        {
            await Init();
            foreach (var id in logIds)
            {
                var log = await _database!.Table<ActivityLog>().FirstOrDefaultAsync(l => l.LogId == id);
                if (log != null)
                {
                    log.IsSynced = 1;
                    await _database.UpdateAsync(log);
                }
            }
        }

        // Delete logs by ids after they have been successfully sent to server
        public async Task DeleteLogsByIdsAsync(List<int> logIds)
        {
            await Init();
            foreach (var id in logIds)
            {
                try
                {
                    var log = await _database!.Table<ActivityLog>().FirstOrDefaultAsync(l => l.LogId == id);
                    if (log != null)
                    {
                        await _database.DeleteAsync(log);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"=== ERROR deleting log {id}: {ex.Message}");
                }
            }
        }

        // ==========================================
        // 5. CÁC HÀM TRUY VẤN (GETTERS / SETTERS)
        // ==========================================
        public async Task<List<POI>> GetPOIsAsync() { await Init(); return await _database!.Table<POI>().ToListAsync(); }

        public async Task<POI> GetPOIByIdAsync(int poiId) { await Init(); return await _database!.Table<POI>().FirstOrDefaultAsync(p => p.PoiId == poiId); }

        public async Task<List<Itinerary>> GetItinerariesAsync() { await Init(); return await _database!.Table<Itinerary>().ToListAsync(); }

        public async Task<int> SavePOIAsync(POI poi) { await Init(); return poi.PoiId != 0 ? await _database!.UpdateAsync(poi) : await _database!.InsertAsync(poi); }

        public async Task<int> DeletePOIAsync(POI poi) { await Init(); return await _database!.DeleteAsync(poi); }

        public async Task<List<ActivityLog>> GetUnSyncedLogsAsync() { await Init(); return await _database!.Table<ActivityLog>().Where(l => l.IsSynced == 0).ToListAsync(); }

        // Menu items feature removed. Methods and table dropped intentionally.
    }
}