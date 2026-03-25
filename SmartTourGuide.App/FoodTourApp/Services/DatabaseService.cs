using SQLite;
using FoodTourApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

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

            // Xóa bảng cũ và tạo mới (để cập nhật schema đa ngôn ngữ)
            await _database.DropTableAsync<POI>();
            await _database.CreateTableAsync<POI>();

            // Nạp dữ liệu mẫu đa ngôn ngữ
            if (await _database.Table<POI>().CountAsync() == 0)
            {
                var vinhKhanhData = GetMultiLanguagePOIs();
                await _database.InsertAllAsync(vinhKhanhData);
            }
        }

        /// <summary>
        /// Dữ liệu POI với 5 ngôn ngữ
        /// </summary>
        private List<POI> GetMultiLanguagePOIs()
        {
            return new List<POI>
            {
                new POI
                {
                    PoiId = 1,
                    Name = "Cổng chào Phố ẩm thực",
                    Category = "Ăn vặt",
                    TriggerRadius = 50,
                    Latitude = 10.75750,
                    Longitude = 106.70700,
                    ImageSource = "cong_chao.jpg",
                    DescriptionVi = "Chào mừng bạn đến với phố ẩm thực Vĩnh Khánh, thiên đường ăn uống về đêm sầm uất nhất Quận 4.",
                    DescriptionEn = "Welcome to Vinh Khanh Food Street, the most vibrant nighttime food paradise in District 4.",
                    DescriptionZh = "欢迎来到永康美食街，这是第四区最繁华的夜间美食天堂。",
                    DescriptionKo = "4군에서 가장 활기찬 야간 음식 천국인 빈칸 푸드 스트리트에 오신 것을 환영합니다.",
                    DescriptionJa = "4区で最も活気のある夜の食の楽園、ヴィンカン・フードストリートへようこそ。"
                },
                new POI
                {
                    PoiId = 2,
                    Name = "Ốc Oanh 534",
                    Category = "Hải sản",
                    TriggerRadius = 35,
                    Latitude = 10.75883,
                    Longitude = 106.70505,
                    ImageSource = "ocoanh.jpg",
                    DescriptionVi = "Hệ thống nhận diện bạn đang đứng trước Ốc Oanh, quán nổi tiếng nhất khu vực với món ốc hương sốt trứng muối đặc trưng.",
                    DescriptionEn = "You are standing in front of Oc Oanh, the most famous restaurant in the area, known for its signature salted egg sauce snail dish.",
                    DescriptionZh = "您正站在Ốc Oanh前面，这是该地区最著名的餐厅，以其招牌咸蛋酱蜗牛菜闻名。",
                    DescriptionKo = "당신은 이 지역에서 가장 유명한 레스토랑인 옥 오안 앞에 서 있습니다. 소금에 절인 달걀 소스 달팽이 요리로 유명합니다.",
                    DescriptionJa = "あなたはこの地域で最も有名なレストラン、オック・オアンの前に立っています。塩卵ソースのカタツムリ料理で知られています。"
                },
                new POI
                {
                    PoiId = 3,
                    Name = "Ốc Vũ 37",
                    Category = "Hải sản",
                    TriggerRadius = 30,
                    Latitude = 10.75916,
                    Longitude = 106.70452,
                    ImageSource = "ocvu.jpg",
                    DescriptionVi = "Bạn đang ở gần Ốc Vũ. Quán nổi tiếng với các loại hải sản tươi sống bắt tại hồ.",
                    DescriptionEn = "You are near Oc Vu. This restaurant is famous for fresh seafood caught directly from the tank.",
                    DescriptionZh = "您在Ốc Vũ附近。这家餐厅以从水箱中直接捕获的新鲜海鲜而闻名。",
                    DescriptionKo = "옥 부 근처에 있습니다. 이 레스토랑은 탱크에서 직접 잡은 신선한 해산물로 유명합니다.",
                    DescriptionJa = "オック・ブの近くにいます。このレストランは、水槽から直接獲れた新鮮なシーフードで有名です。"
                },
                new POI
                {
                    PoiId = 4,
                    Name = "Lẩu bò Khu Nhà Cháy",
                    Category = "Món Lẩu",
                    TriggerRadius = 40,
                    Latitude = 10.75822,
                    Longitude = 106.70611,
                    ImageSource = "laubo.jpg",
                    DescriptionVi = "Bạn đang tiến vào khu vực quán Lẩu bò Nhà Cháy. Đây là địa điểm ăn uống lâu đời với nước dùng đậm đà.",
                    DescriptionEn = "You are entering the Nha Chay Beef Hotpot area. This is a long-established eatery with rich and flavorful broth.",
                    DescriptionZh = "您正在进入Nhà Cháy牛肉火锅区。这是一家历史悠久的餐厅，汤底浓郁可口。",
                    DescriptionKo = "나차이 소고기 훠궈 구역으로 들어가고 있습니다. 풍부하고 맛있는 육수로 유명한 오래된 식당입니다.",
                    DescriptionJa = "ニャチャイ牛肉鍋エリアに入っています。濃厚で風味豊かなスープで知られる老舗の飲食店です。"
                },
                new POI
                {
                    PoiId = 5,
                    Name = "Sữa tươi chiên",
                    Category = "Ăn vặt",
                    TriggerRadius = 25,
                    Latitude = 10.75850,
                    Longitude = 106.70555,
                    ImageSource = "suatuoi.jpg",
                    DescriptionVi = "Hệ thống gợi ý bạn thử món sữa tươi chiên ngay phía trước. Những viên sữa được chiên vàng giòn bên ngoài.",
                    DescriptionEn = "We suggest you try the fried fresh milk right ahead. The milk pieces are fried golden and crispy on the outside.",
                    DescriptionZh = "我们建议您尝试前面的炸鲜奶。牛奶块外面炸得金黄酥脆。",
                    DescriptionKo = "앞에 있는 튀긴 우유를 맛보시길 권합니다. 우유 조각은 겉이 황금색으로 바삭하게 튀겨집니다.",
                    DescriptionJa = "目の前にある揚げミルクを試してみることをお勧めします。ミルクは外側がきつね色でカリカリに揚げられています。"
                },
                new POI
                {
                    PoiId = 6,
                    Name = "Phá lấu Dì Nũi",
                    Category = "Ăn vặt",
                    TriggerRadius = 30,
                    Latitude = 10.75940,
                    Longitude = 106.70410,
                    ImageSource = "phalau.jpg",
                    DescriptionVi = "Bạn đang ở gần hẻm phá lấu Dì Nũi nổi tiếng. Với hơn 20 năm kinh nghiệm, chén phá lấu ở đây béo ngậy nước cốt dừa.",
                    DescriptionEn = "You are near the famous Di Nui Pha Lau alley. With over 20 years of experience, the pha lau here is rich with coconut milk.",
                    DescriptionZh = "您在著名的Dì Nũi Phá Lấu小巷附近。凭借20多年的经验，这里的phá lấu富含椰奶。",
                    DescriptionKo = "유명한 디 누이 파라우 골목 근처에 있습니다. 20년 이상의 경험으로, 이곳의 파라우는 코코넛 밀크가 풍부합니다.",
                    DescriptionJa = "有名なディ・ヌイ・ファーラウ路地の近くにいます。20年以上の経験を持ち、ここのファーラウはココナッツミルクが豊富です。"
                },
                new POI
                {
                    PoiId = 7,
                    Name = "Sushi KO",
                    Category = "Đồ Nhật",
                    TriggerRadius = 45,
                    Latitude = 10.75800,
                    Longitude = 106.70650,
                    ImageSource = "sushi.jpg",
                    DescriptionVi = "Phía trước bạn là Sushi KO. Một địa điểm thú vị với các món Nhật Bản giá bình dân.",
                    DescriptionEn = "In front of you is Sushi KO. An interesting place with affordable Japanese dishes.",
                    DescriptionZh = "在您面前是Sushi KO。一个有趣的地方，提供价格实惠的日本料理。",
                    DescriptionKo = "앞에 스시 KO가 있습니다. 저렴한 일본 요리를 제공하는 흥미로운 장소입니다.",
                    DescriptionJa = "目の前にスシKOがあります。手頃な価格の日本料理を提供する興味深い場所です。"
                },
                new POI
                {
                    PoiId = 8,
                    Name = "Bún mắm 135",
                    Category = "Hải sản",
                    TriggerRadius = 30,
                    Latitude = 10.75860,
                    Longitude = 106.70580,
                    ImageSource = "bunmam.jpg",
                    DescriptionVi = "Chào mừng bạn đến với Bún mắm 135. Tô bún ở đây đầy đặn với tôm, mực, heo quay.",
                    DescriptionEn = "Welcome to Bun Mam 135. The noodle bowl here is full of shrimp, squid, and roasted pork.",
                    DescriptionZh = "欢迎来到Bún Mắm 135。这里的面碗里满是虾、鱿鱼和烤猪肉。",
                    DescriptionKo = "분맘 135에 오신 것을 환영합니다. 이곳의 국수 그릇에는 새우, 오징어, 구운 돼지고기가 가득합니다.",
                    DescriptionJa = "ブンマム135へようこそ。ここの麺丼にはエビ、イカ、ローストポークがたっぷり入っています。"
                },
                new POI
                {
                    PoiId = 9,
                    Name = "Trà sữa túi lọc",
                    Category = "Giải khát",
                    TriggerRadius = 25,
                    Latitude = 10.75900,
                    Longitude = 106.70480,
                    ImageSource = "trasua.jpg",
                    DescriptionVi = "Bạn đã đi gần đến khu vực giải khát. Hãy thưởng thức một ly trà sữa túi lọc truyền thống.",
                    DescriptionEn = "You are near the refreshment area. Enjoy a traditional filter bag milk tea.",
                    DescriptionZh = "您已接近饮料区。享用一杯传统的袋泡奶茶。",
                    DescriptionKo = "음료 구역 근처에 있습니다. 전통 필터백 밀크티를 즐겨보세요.",
                    DescriptionJa = "ドリンクエリアの近くにいます。伝統的なフィルターバッグミルクティーをお楽しみください。"
                },
                new POI
                {
                    PoiId = 10,
                    Name = "Xôi gà hẻm 200",
                    Category = "Ăn vặt",
                    TriggerRadius = 35,
                    Latitude = 10.75960,
                    Longitude = 106.70380,
                    ImageSource = "xoiga.jpg",
                    DescriptionVi = "Nếu bạn muốn đổi vị, hãy thử xôi gà tại hẻm 200. Xôi ở đây dẻo thơm, gà được chiên giòn rụm.",
                    DescriptionEn = "If you want a change, try the chicken sticky rice at alley 200. The sticky rice here is fragrant and the chicken is crispy fried.",
                    DescriptionZh = "如果您想换口味，可以尝试200巷的鸡肉糯米饭。这里的糯米饭香糯，鸡肉炸得酥脆。",
                    DescriptionKo = "색다른 맛을 원하시면 200번 골목의 치킨 찹쌀밥을 맛보세요. 이곳의 찹쌀밥은 향긋하고 치킨은 바삭하게 튀겨집니다.",
                    DescriptionJa = "気分転換したいなら、200番路地のチキンもち米をお試しください。ここのもち米は香り高く、チキンはカリカリに揚げられています。"
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
    }
}