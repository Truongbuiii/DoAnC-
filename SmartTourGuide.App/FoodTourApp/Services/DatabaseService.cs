using SQLite;
using FoodTourApp.Models;
using System.Diagnostics;

namespace FoodTourApp.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

        // ==========================================
        // 1. KHỞI TẠO DATABASE (MIGRATION)
        // ==========================================
        private async Task Init()
        {
            if (_database is not null) return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "VinhKhanhTour.db3");
            _database = new SQLiteAsyncConnection(dbPath);

            await _database.CreateTableAsync<POI>();
            await _database.CreateTableAsync<Itinerary>();
            await _database.CreateTableAsync<MenuItemModel>(); // <--- DÒNG NÀY PHẢI CÓ
            await _database.CreateTableAsync<Tour>();
            await _database.CreateTableAsync<ActivityLog>();

            // XÓA HOẶC COMMENT 2 DÒNG NÀY ĐỂ KHÔNG DÙNG DỮ LIỆU MẪU NỮA
            // if (await _database.Table<POI>().CountAsync() == 0)
            //    await _database.InsertAllAsync(GetSeedPOIs());

            // if (await _database.Table<Itinerary>().CountAsync() == 0)
            //    await _database.InsertAllAsync(GetSampleItineraries());
        }

        // ==========================================
        // 2. LOGIC DỊCH THUẬT & CACHE (ĐÃ CHUẨN HÓA VI, EN, ZH, KO, JA)
        // ==========================================
        public async Task TranslateAndCachePoisAsync()
        {
            await Init();

            // Sửa lại: Nếu THIẾU TIẾNG NHẬT thì mới dịch (vì tiếng Anh Duy đã nạp sẵn rồi)
            var pois = await _database.Table<POI>()
                .Where(p => p.DescriptionJa == null || p.DescriptionJa == "" || p.DescriptionJa.Contains("..."))
                .ToListAsync();

            if (!pois.Any())
            {
                Debug.WriteLine("=== KHÔNG CÓ POI MỚI CẦN DỊCH ===");
                return;
            }

            Debug.WriteLine($"=== TIẾN HÀNH DỊCH {pois.Count} QUÁN ĂN ===");
            var translator = new TranslationService();

            foreach (var poi in pois)
            {
                try
                {
                    // Thực hiện dịch tuần tự từng quán
                    await translator.TranslatePoiAsync(poi);

                    // Lưu ngay bản dịch vào SQLite để không bị mất nếu văng App
                    await _database.UpdateAsync(poi);

                    Debug.WriteLine($"=== HOÀN TẤT DỊCH: {poi.Name}");

                    // Nghỉ 1.2 giây để "né" bộ quét bot của MyMemory/Google
                    await Task.Delay(1200);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"=== LỖI DỊCH TẠI {poi.Name}: {ex.Message}");
                }
            }
            Debug.WriteLine("=== ĐÃ HOÀN TẤT TOÀN BỘ BẢN DỊCH ===");
        }

        // ==========================================
        // 3. ĐỒNG BỘ DỮ LIỆU TỪ WEB ADMIN SERVER
        // ==========================================
        public async Task SavePOIsFromServerAsync(List<POI> pois)
        {
            await Init();
            foreach (var serverPoi in pois)
            {
                // 1. Tìm xem quán này đã có trong máy chưa
                var localPoi = await _database.Table<POI>()
                    .FirstOrDefaultAsync(p => p.PoiId == serverPoi.PoiId);

                if (localPoi != null)
                {
                    // 2. MẸO: Nếu server trả về NULL bản dịch, ta giữ lại bản cũ trong máy
                    serverPoi.DescriptionEn ??= localPoi.DescriptionEn;
                    serverPoi.DescriptionZh ??= localPoi.DescriptionZh;
                    serverPoi.DescriptionKo ??= localPoi.DescriptionKo;
                    serverPoi.DescriptionJa ??= localPoi.DescriptionJa;
                }

                // 3. Lúc này mới lưu đè vào SQLite
                await _database.InsertOrReplaceAsync(serverPoi);
                Debug.WriteLine($"=== ĐÃ ĐỒNG BỘ {pois.Count} QUÁN TỪ WEB ADMIN ===");
            }
        }

        public async Task SaveToursFromServerAsync(List<Itinerary> tours)
        {
            await Init();
            foreach (var tour in tours)
                await _database.InsertOrReplaceAsync(tour);
        }

        // ==========================================
        // 4. QUẢN LÝ NHẬT KÝ (LOGS) & ĐỒNG BỘ
        // ==========================================
        public async Task LogActivityAsync(int poiId, string actionType, string language)
        {
            await Init();
            var log = new ActivityLog
            {
                PoiId = poiId,
                ActionType = actionType,
                LanguageUsed = language, // Sẽ lưu là VI, EN, ZH, KO, JA
                DeviceType = "Android",
                AccessTime = DateTime.Now,
                IsSynced = 0
            };
            await _database.InsertAsync(log);
        }

        public async Task MarkLogsAsSyncedAsync(List<int> logIds)
        {
            await Init();
            foreach (var id in logIds)
            {
                var log = await _database.Table<ActivityLog>().FirstOrDefaultAsync(l => l.LogId == id);
                if (log != null)
                {
                    log.IsSynced = 1;
                    await _database.UpdateAsync(log);
                }
            }
        }

        // ==========================================
        // 5. CÁC HÀM TRUY VẤN DỮ LIỆU (GETTER)
        // ==========================================
        public async Task<List<POI>> GetPOIsAsync() { await Init(); return await _database.Table<POI>().ToListAsync(); }

        public async Task<POI> GetPOIByIdAsync(int poiId) { await Init(); return await _database.Table<POI>().FirstOrDefaultAsync(p => p.PoiId == poiId); }

        public async Task<List<Itinerary>> GetItinerariesAsync() { await Init(); return await _database.Table<Itinerary>().ToListAsync(); }

        public async Task<int> SavePOIAsync(POI poi) { await Init(); return poi.PoiId != 0 ? await _database.UpdateAsync(poi) : await _database.InsertAsync(poi); }

        public async Task<int> DeletePOIAsync(POI poi) { await Init(); return await _database.DeleteAsync(poi); }

        public async Task<List<ActivityLog>> GetUnSyncedLogsAsync() { await Init(); return await _database.Table<ActivityLog>().Where(l => l.IsSynced == 0).ToListAsync(); }

        public async Task<List<MenuItemModel>> GetMenuItemsByPoiIdAsync(int poiId)
        {
            await Init(); // Cực kỳ quan trọng: Phải khởi tạo DB trước
            var list = await _database.Table<MenuItemModel>()
                                      .Where(m => m.PoiId == poiId)
                                      .ToListAsync();

            System.Diagnostics.Debug.WriteLine($"=== POI {poiId} có {list.Count} món ăn");
            return list;
        }
    }
}