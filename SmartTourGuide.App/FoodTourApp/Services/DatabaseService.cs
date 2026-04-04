using SQLite;
using FoodTourApp.Models;

namespace FoodTourApp.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

        private async Task Init()
        {
            if (_database is not null) return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "VinhKhanhTour.db3");
            _database = new SQLiteAsyncConnection(dbPath);

            // Tạo bảng nếu chưa có (không xóa data cũ)
            await _database.CreateTableAsync<POI>();
            await _database.CreateTableAsync<Itinerary>();
            await _database.CreateTableAsync<ActivityLog>();

            // Seed data nếu bảng trống
            if (await _database.Table<POI>().CountAsync() == 0)
                await _database.InsertAllAsync(GetMultiLanguagePOIs());

            if (await _database.Table<Itinerary>().CountAsync() == 0)
                await _database.InsertAllAsync(GetSampleItineraries());
        }
public async Task SavePOIsFromServerAsync(List<POI> pois)
{
    await Init();
    foreach (var poi in pois)
        await _database.InsertOrReplaceAsync(poi);
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

        private List<POI> GetMultiLanguagePOIs()
        {
            return new List<POI>
            {
                new POI { PoiId=1, Name="Cổng chào Phố ẩm thực", Category="Điểm tham quan", TriggerRadius=50, Latitude=10.761858, Longitude=106.702236, ImageSource="congchao.jpg",
                    DescriptionVi="Chào mừng bạn đến với phố ẩm thực Vĩnh Khánh, thiên đường ăn uống về đêm sầm uất nhất Quận 4, lọt top 10 con phố thú vị nhất thế giới 2025.",
                    DescriptionEn="Welcome to Vinh Khanh Food Street, the most vibrant nighttime food paradise in District 4, ranked top 10 most exciting streets in the world 2025.",
                    DescriptionZh="欢迎来到永康美食街，这是第四区最繁华的夜间美食天堂，荣登2025年全球最有趣街道前十名。",
                    DescriptionKo="2025년 세계에서 가장 흥미로운 거리 10위에 선정된 4군 최고의 야간 음식 천국에 오신 것을 환영합니다.",
                    DescriptionJa="2025年世界で最も魅力的な通りトップ10に選ばれた、4区最大の夜の食の楽園へようこそ。" },

                new POI { PoiId=2, Name="Dê Chung", Category="Lẩu dê", TriggerRadius=35, Latitude=10.761500, Longitude=106.702600, ImageSource="laubo.jpg",
                    DescriptionVi="Dê Chung tại số 3 Vĩnh Khánh nổi tiếng với món lẩu dê thơm ngon đặc trưng. Quán mở cửa từ chiều đến đêm khuya, rất đông khách địa phương.",
                    DescriptionEn="De Chung at 3 Vinh Khanh is famous for its aromatic goat hotpot, open from afternoon until late night.",
                    DescriptionZh="位于永康3号的Dê Chung以其香气扑鼻的山羊火锅而闻名，从下午营业至深夜。",
                    DescriptionKo="빈칸 3번지의 Dê Chung은 향긋한 염소 샤부샤부로 유명합니다. 오후부터 늦은 밤까지 영업합니다.",
                    DescriptionJa="ヴィンカン通り3番地のDê Chungは香り豊かなヤギ鍋で有名です。午後から深夜まで営業しています。" },

                new POI { PoiId=3, Name="Ốc Vũ 37", Category="Hải sản", TriggerRadius=35, Latitude=10.761403, Longitude=106.702705, ImageSource="ocvu.jpg",
                    DescriptionVi="Ốc Vũ tại số 37 Vĩnh Khánh là quán ốc chuẩn local của Quận 4, nổi tiếng với ốc len xào dừa và nước chấm tự pha độc quyền.",
                    DescriptionEn="Oc Vu at 37 Vinh Khanh is an authentic local seafood restaurant, famous for stir-fried snails with coconut.",
                    DescriptionZh="位于永康37号的Ốc Vũ是第四区正宗的当地海鲜餐厅，以椰子炒蜗牛而闻名。",
                    DescriptionKo="빈칸 37번지의 옥 부는 4군의 정통 로컬 해산물 식당으로, 코코넛 볶음 달팽이로 유명합니다.",
                    DescriptionJa="ヴィンカン通り37番地のOc Vuは4区の本格的な地元海鮮レストランです。" },

                new POI { PoiId=4, Name="Bún cá Châu Đốc Dì Tư", Category="Bún cá", TriggerRadius=30, Latitude=10.761200, Longitude=106.703100, ImageSource="bunmam.jpg",
                    DescriptionVi="Bún cá Châu Đốc Dì Tư tại số 75 Vĩnh Khánh mang hương vị miền Tây đặc trưng. Mỗi tô bún là sự kết hợp hài hòa giữa cá tươi, bún mềm và rau sống.",
                    DescriptionEn="Bun Ca Chau Doc Di Tu at 75 Vinh Khanh brings authentic Mekong Delta flavors.",
                    DescriptionZh="位于永康75号的Bún Cá Châu Đốc Dì Tư带来正宗湄公河三角洲风味。",
                    DescriptionKo="빈칸 75번지의 Bún Cá Châu Đốc Dì Tư는 메콩 델타의 정통 맛을 선사합니다.",
                    DescriptionJa="ヴィンカン通り75番地のBún Cá Châu Đốc Dì Tưは本格的なメコンデルタの味を提供します。" },

                new POI { PoiId=5, Name="Ốc Thảo 123", Category="Hải sản", TriggerRadius=35, Latitude=10.760750, Longitude=106.704600, ImageSource="ocoanh.jpg",
                    DescriptionVi="Ốc Thảo tại số 123 Vĩnh Khánh là thiên đường hải sản đa dạng nhất phố. Quán có hơn 30 loại ốc tươi sống, giá bình dân.",
                    DescriptionEn="Oc Thao at 123 Vinh Khanh is the most diverse seafood paradise on the street.",
                    DescriptionZh="位于永康123号的Ốc Thảo是该街道最多样化的海鲜天堂。",
                    DescriptionKo="빈칸 123번지의 옥 타오는 거리에서 가장 다양한 해산물 천국입니다.",
                    DescriptionJa="ヴィンカン通り123番地のOc Thaoは通りで最も多様な海鮮の楽園です。" },

                new POI { PoiId=6, Name="Sushi KO", Category="Đồ Nhật", TriggerRadius=30, Latitude=10.760739, Longitude=106.704651, ImageSource="sushi.jpg",
                    DescriptionVi="Sushi KO tại số 122 Vĩnh Khánh phục vụ các món Nhật Bản sáng tạo với giá bình dân.",
                    DescriptionEn="Sushi KO at 122 Vinh Khanh serves creative Japanese dishes at affordable prices.",
                    DescriptionZh="位于永康122号的Sushi KO以实惠的价格提供创意日本料理。",
                    DescriptionKo="빈칸 122번지의 스시 KO는 저렴한 가격에 창의적인 일본 요리를 제공합니다.",
                    DescriptionJa="ヴィンカン通り122番地のSushi KOはリーズナブルな価格で創作日本料理を提供しています。" },

                new POI { PoiId=7, Name="Chilli Lẩu Nướng 232", Category="Lẩu nướng", TriggerRadius=35, Latitude=10.760900, Longitude=106.703800, ImageSource="laubo.jpg",
                    DescriptionVi="Chilli Lẩu Nướng tại số 232 Vĩnh Khánh phục vụ lẩu và nướng tự chọn giá từ 59.000đ. Mở đến 3h sáng.",
                    DescriptionEn="Chilli Hot Pot & Grill at 232 Vinh Khanh serves all-you-can-choose dishes from 59,000 VND. Open until 3 AM.",
                    DescriptionZh="位于永康232号的Chilli火锅烧烤提供从59,000越盾起的自选火锅和烧烤。",
                    DescriptionKo="빈칸 232번지의 칠리 훠궈 & 그릴은 59,000동부터 시작하는 셀프 훠궈와 구이를 제공합니다.",
                    DescriptionJa="ヴィンカン通り232番地のChilli鍋&グリルは59,000ドンからのセルフ鍋と焼き料理を提供しています。" },

                new POI { PoiId=8, Name="Ốc Đào 2", Category="Hải sản", TriggerRadius=30, Latitude=10.760820, Longitude=106.703500, ImageSource="ocvu.jpg",
                    DescriptionVi="Ốc Đào 2 tại số 232/123 Vĩnh Khánh đặc biệt nổi tiếng với sốt trứng muối và xào me đậm vị.",
                    DescriptionEn="Oc Dao 2 at 232/123 Vinh Khanh is especially famous for its salted egg sauce and tamarind stir-fry.",
                    DescriptionZh="位于永康232/123号的Ốc Đào 2尤其以咸蛋酱和酸角炒而闻名。",
                    DescriptionKo="빈칸 232/123번지의 옥 다오 2는 소금 달걀 소스와 타마린드 볶음으로 유명합니다.",
                    DescriptionJa="ヴィンカン通り232/123番地のOc Dao 2は塩卵ソースとタマリンド炒めで有名です。" },

                new POI { PoiId=9, Name="Ốc Thảo 383", Category="Hải sản", TriggerRadius=30, Latitude=10.760770, Longitude=106.703400, ImageSource="ocoanh.jpg",
                    DescriptionVi="Ốc Thảo tại số 383 Vĩnh Khánh có không gian rộng rãi, hải sản tươi ngon mỗi ngày.",
                    DescriptionEn="Oc Thao at 383 Vinh Khanh has a spacious setting with fresh seafood daily.",
                    DescriptionZh="位于永康383号的Ốc Thảo空间宽敞，每日提供新鲜海鲜。",
                    DescriptionKo="빈칸 383번지의 옥 타오는 넓은 공간과 매일 신선한 해산물을 제공합니다.",
                    DescriptionJa="ヴィンカン通り383番地のOc Thaoは広いスペースで毎日新鮮な海鮮を提供します。" },

                new POI { PoiId=10, Name="Ốc Oanh 534", Category="Hải sản", TriggerRadius=40, Latitude=10.760719, Longitude=106.703297, ImageSource="ocoanh.jpg",
                    DescriptionVi="Ốc Oanh tại số 534 Vĩnh Khánh là quán ốc nổi tiếng nhất phố, được Michelin Bib Gourmand 2024 vinh danh.",
                    DescriptionEn="Oc Oanh at 534 Vinh Khanh is the most famous restaurant on the street, awarded Michelin Bib Gourmand 2024.",
                    DescriptionZh="位于永康534号的Ốc Oanh荣获2024年米其林必比登推介。",
                    DescriptionKo="빈칸 534번지의 옥 오안은 2024 미슐랭 빕 구르망을 수상했습니다.",
                    DescriptionJa="ヴィンカン通り534番地のOc Oanhは2024年ミシュランビブグルマンを受賞しました。" }
            };
        }

        private List<Itinerary> GetSampleItineraries()
        {
            return new List<Itinerary>
    {
        new Itinerary
        {
            TourId = 1,  // thêm TourId
            TourName = "Tour Ốc Michelin & Đặc Sản",
            Description = "Hành trình thưởng thức những quán ốc được Michelin vinh danh và các quán lâu đời nhất phố Vĩnh Khánh.",
            TotalTime = "2 giờ 30 phút · 4 điểm",
            ImageSource = "ocoanh.jpg",
            PoiIds = new List<int> { 1, 3, 8, 10 }
        },
        new Itinerary
        {
            TourId = 2,  // thêm TourId
            TourName = "Lộ Trình Ăn Đêm Sầm Uất",
            Description = "Dạo quanh các điểm ăn uống nhộn nhịp từ đầu phố đến cuối phố, bao gồm Lẩu và Nướng.",
            TotalTime = "3 giờ · 4 điểm",
            ImageSource = "laubo.jpg",
            PoiIds = new List<int> { 2, 4, 7, 9 }
        },
        new Itinerary
        {
            TourId = 3,  // thêm TourId
            TourName = "Tour Hải Sản Đa Quốc Gia",
            Description = "Trải nghiệm giao thoa văn hóa ẩm thực giữa Hải sản Việt Nam và Sushi Nhật Bản.",
            TotalTime = "2 giờ · 3 điểm",
            ImageSource = "sushi.jpg",
            PoiIds = new List<int> { 1, 5, 6 }
        }
    };
        }

        public async Task<List<POI>> GetPOIsAsync()
        {
            await Init();
            return await _database.Table<POI>().ToListAsync();
        }

        public async Task<POI> GetPOIByIdAsync(int poiId)
        {
            await Init();
            return await _database.Table<POI>().FirstOrDefaultAsync(p => p.PoiId == poiId);
        }

        public async Task<List<Itinerary>> GetItinerariesAsync()
        {
            await Init();
            return await _database.Table<Itinerary>().ToListAsync();
        }

        public async Task<int> SavePOIAsync(POI poi)
        {
            await Init();
            return poi.PoiId != 0 ? await _database.UpdateAsync(poi) : await _database.InsertAsync(poi);
        }

        public async Task<int> DeletePOIAsync(POI poi)
        {
            await Init();
            return await _database.DeleteAsync(poi);
        }
        public async Task LogActivityAsync(int poiId, string actionType, string language)
        {
            await Init();
            var log = new ActivityLog
            {
                PoiId = poiId,
                ActionType = actionType,
                LanguageUsed = language,
                DeviceType = "Android",
                AccessTime = DateTime.Now,
                IsSynced = 0
            };
            await _database.InsertAsync(log);
        }

        public async Task<List<ActivityLog>> GetUnSyncedLogsAsync()
        {
            await Init();
            return await _database.Table<ActivityLog>()
                .Where(l => l.IsSynced == 0)
                .ToListAsync();
        }
    }
}