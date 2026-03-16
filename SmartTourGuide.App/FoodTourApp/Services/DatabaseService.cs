using SQLite;
using FoodTourApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO; // Bắt buộc phải có để dùng Path.Combine

namespace FoodTourApp.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

        // Chuyển thành private để chỉ sử dụng nội bộ trong class
        private async Task Init()
        {
            if (_database is not null) return;

            // Đường dẫn file database trên Windows
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "VinhKhanhTour.db3");
            _database = new SQLiteAsyncConnection(dbPath);
            await _database.CreateTableAsync<POI>();

            // Tự động nạp 10 địa điểm Vĩnh Khánh nếu DB trống
            if (await _database.Table<POI>().CountAsync() == 0)
            {
                var vinhKhanhData = new List<POI>
                {
                    new POI { PoiId = 1, Name = "Cổng chào Phố ẩm thực", Category = "Ăn vặt", TriggerRadius = 50, Latitude = 10.75750, Longitude = 106.70700, ImageSource = "cong_chao.jpg", Description = "Chào mừng bạn đến với phố ẩm thực Vĩnh Khánh, thiên đường ăn uống về đêm sầm uất nhất Quận 4." },
                    new POI { PoiId = 2, Name = "Ốc Oanh 534", Category = "Hải sản", TriggerRadius = 35, Latitude = 10.75883, Longitude = 106.70505, ImageSource = "ocoanh.jpg", Description = "Hệ thống nhận diện bạn đang đứng trước Ốc Oanh, quán nổi tiếng nhất khu vực với món ốc hương sốt trứng muối đặc trưng." },
                    new POI { PoiId = 3, Name = "Ốc Vũ 37", Category = "Hải sản", TriggerRadius = 30, Latitude = 10.75916, Longitude = 106.70452, ImageSource = "ocvu.jpg", Description = "Bạn đang ở gần Ốc Vũ. Quán nổi tiếng với các loại hải sản tươi sống bắt tại hồ." },
                    new POI { PoiId = 4, Name = "Lẩu bò Khu Nhà Cháy", Category = "Món Lẩu", TriggerRadius = 40, Latitude = 10.75822, Longitude = 106.70611, ImageSource = "laubo.jpg", Description = "Bạn đang tiến vào khu vực quán Lẩu bò Nhà Cháy. Đây là địa điểm ăn uống lâu đời với nước dùng đậm đà." },
                    new POI { PoiId = 5, Name = "Sữa tươi chiên", Category = "Ăn vặt", TriggerRadius = 25, Latitude = 10.75850, Longitude = 106.70555, ImageSource = "suatuoi.jpg", Description = "Hệ thống gợi ý bạn thử món sữa tươi chiên ngay phía trước. Những viên sữa được chiên vàng giôm bên ngoài." },
                    new POI { PoiId = 6, Name = "Phá lấu Dì Nũi", Category = "Ăn vặt", TriggerRadius = 30, Latitude = 10.75940, Longitude = 106.70410, ImageSource = "phalau.jpg", Description = "Bạn đang ở gần hẻm phá lấu Dì Nũi nổi tiếng. Với hơn 20 năm kinh nghiệm, chén phá lấu ở đây béo ngậy nước cốt dừa." },
                    new POI { PoiId = 7, Name = "Sushi KO", Category = "Đồ Nhật", TriggerRadius = 45, Latitude = 10.75800, Longitude = 106.70650, ImageSource = "sushi.jpg", Description = "Phía trước bạn là Sushi KO. Một địa điểm thú vị với các món Nhật Bản giá bình dân." },
                    new POI { PoiId = 8, Name = "Bún mắm 135", Category = "Hải sản", TriggerRadius = 30, Latitude = 10.75860, Longitude = 106.70580, ImageSource = "bunmam.jpg", Description = "Chào mừng bạn đến với Bún mắm 135. Tô bún ở đây đầy đặn với tôm, mực, heo quay." },
                    new POI { PoiId = 9, Name = "Trà sữa túi lọc", Category = "Giải khát", TriggerRadius = 25, Latitude = 10.75900, Longitude = 106.70480, ImageSource = "trasua.jpg", Description = "Bạn đã đi gần đến khu vực giải khát. Hãy thưởng thức một ly trà sữa túi lọc truyền thống." },
                    new POI { PoiId = 10, Name = "Xôi gà hẻm 200", Category = "Ăn vặt", TriggerRadius = 35, Latitude = 10.75960, Longitude = 106.70380, ImageSource = "xoiga.jpg", Description = "Nếu bạn muốn đổi vị, hãy thử xôi gà tại hẻm 200. Xôi ở đây dẻo thơm, gà được chiên giòn rụm." }
                };
                await _database.InsertAllAsync(vinhKhanhData);
            }
        }

        public async Task<List<POI>> GetPOIsAsync()
        {
            await Init();
            return await _database.Table<POI>().ToListAsync();
        }

        public async Task<int> SavePOIAsync(POI poi)
        {
            await Init();
            // ĐÃ SỬA: Thay Id bằng PoiId để khớp với Model của bạn
            return poi.PoiId != 0 ? await _database.UpdateAsync(poi) : await _database.InsertAsync(poi);
        }
    }
}