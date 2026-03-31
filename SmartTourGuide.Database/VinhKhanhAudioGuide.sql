USE VinhKhanhAudioGuide;
GO

-- ==========================================
-- 1. DỌN DẸP SẠCH SẼ CÁC BẢNG CŨ ĐỂ KHÔNG BỊ TRÙNG
-- Lưu ý: Phải xóa bảng con trước, bảng cha sau
-- ==========================================
DROP TABLE IF EXISTS ActivityLogs;
DROP TABLE IF EXISTS TourDetails;
DROP TABLE IF EXISTS Audios;
DROP TABLE IF EXISTS Tours;
DROP TABLE IF EXISTS POIs;
DROP TABLE IF EXISTS Admins; -- Xóa luôn bảng Admins cũ nếu có
GO

-- ==========================================
-- 2. TẠO LẠI CẤU TRÚC BẢNG MỚI TINH
-- ==========================================

-- Tạo lại bảng Admins (Nếu Tài dùng để đăng nhập Web)
CREATE TABLE Admins (
    AdminId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100)
);

CREATE TABLE POIs (
    PoiId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255) NOT NULL,
    Category NVARCHAR(100),
    Latitude FLOAT,
    Longitude FLOAT,
    TriggerRadius FLOAT,
    ImageSource NVARCHAR(MAX),
    DescriptionVi NVARCHAR(MAX),
    DescriptionEn NVARCHAR(MAX),
    DescriptionZh NVARCHAR(MAX),
    DescriptionKo NVARCHAR(MAX),
    DescriptionJa NVARCHAR(MAX)
);

CREATE TABLE Audios (
    AudioId INT PRIMARY KEY IDENTITY(1,1),
    AudioName NVARCHAR(255),
    [Description] NVARCHAR(MAX),
    FilePath NVARCHAR(MAX),
    [Language] NVARCHAR(50),
    PoiId INT FOREIGN KEY REFERENCES POIs(PoiId) ON DELETE CASCADE
);

CREATE TABLE Tours (
    TourId INT PRIMARY KEY IDENTITY(1,1),
    TourName NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(MAX),
    TotalTime NVARCHAR(50)
);

CREATE TABLE TourDetails (
    TourDetailId INT PRIMARY KEY IDENTITY(1,1),
    TourId INT FOREIGN KEY REFERENCES Tours(TourId) ON DELETE CASCADE,
    PoiId INT FOREIGN KEY REFERENCES POIs(PoiId) ON DELETE CASCADE,
    [Order] INT
);

CREATE TABLE ActivityLogs (
    LogId INT PRIMARY KEY IDENTITY(1,1),
    PoiId INT FOREIGN KEY REFERENCES POIs(PoiId) ON DELETE CASCADE,
    ActionType NVARCHAR(50), 
    LanguageUsed NVARCHAR(10),
    DeviceType NVARCHAR(50), 
    AccessTime DATETIME DEFAULT GETDATE()
);
GO

-- ==========================================
-- 3. NẠP DỮ LIỆU
-- ==========================================

-- Nạp 1 tài khoản Admin mặc định
INSERT INTO Admins (Username, Password, FullName) VALUES ('admin', '123456', N'Nguyễn Đức Tài');

-- Nạp 10 Địa điểm (POIs)
INSERT INTO POIs (Name, Category, TriggerRadius, Latitude, Longitude, ImageSource, DescriptionVi, DescriptionEn, DescriptionZh, DescriptionKo, DescriptionJa) VALUES 
(N'Cổng chào Phố ẩm thực', N'Điểm tham quan', 50, 10.761858, 106.702236, 'congchao.jpg', N'Chào mừng bạn đến với phố ẩm thực Vĩnh Khánh, thiên đường ăn uống về đêm sầm uất nhất Quận 4, lọt top 10 con phố thú vị nhất thế giới 2025.', N'Welcome to Vinh Khanh Food Street, the most vibrant nighttime food paradise in District 4, ranked top 10 most exciting streets in the world 2025.', N'欢迎来到永康美食街，这是第四区最繁华的夜间美食天堂，荣登2025年全球最有趣街道前十名。', N'2025년 세계에서 가장 흥미로운 거리 10위에 선정된 4군 최고의 야간 음식 천국, 빈칸 푸드 스트리트에 오신 것을 환영합니다.', N'2025年世界で最も魅力的な通りトップ10に選ばれた、4区最大の夜の食の楽園、ヴィンカン・フードストリートへようこそ。'),
(N'Dê Chung', N'Lẩu dê', 35, 10.761500, 106.702600, 'laubo.jpg', N'Dê Chung tại số 3 Vĩnh Khánh nổi tiếng với món lẩu dê thơm ngon đặc trưng. Quán mở cửa từ chiều đến đêm khuya.', N'De Chung at 3 Vinh Khanh is famous for its aromatic goat hotpot. The restaurant is open from afternoon until late night.', N'位于永康3号的Dê Chung以其香气扑鼻的山羊火锅而闻名。', N'빈칸 3번지에 위치한 Dê Chung은 향긋한 염소 샤부샤부로 유명합니다.', N'ヴィンカン通り3番地のDê Chungは香り豊かなヤギ鍋で有名です。'),
(N'Ốc Vũ 37', N'Hải sản', 35, 10.761403, 106.702705, 'ocvu.jpg', N'Ốc Vũ tại số 37 Vĩnh Khánh là quán ốc chuẩn local của Quận 4, nổi tiếng với ốc len xào dừa.', N'Oc Vu at 37 Vinh Khanh is an authentic local seafood restaurant in District 4, famous for stir-fried snails with coconut.', N'位于永康37号的Ốc Vũ是第四区正宗的当地海鲜餐厅，以椰子炒蜗牛而闻名。', N'빈칸 37번지의 옥 부는 4군의 정통 로컬 해산물 식당으로, 코코넛 볶음 달팽이로 유명합니다.', N'ヴィンカン通り37番地のOc Vuは4区の本格的な地元海鮮レストランです。'),
(N'Bún cá Châu Đốc Dì Tư', N'Bún cá', 30, 10.761200, 106.703100, 'bunmam.jpg', N'Bún cá Châu Đốc Dì Tư tại số 75 Vĩnh Khánh mang hương vị miền Tây đặc trưng.', N'Bun Ca Chau Doc Di Tu at 75 Vinh Khanh brings authentic Mekong Delta flavors.', N'位于永康75号的Bún Cá Châu Đốc Dì Tư带来正宗湄公河三角洲风味。', N'빈칸 75번지의 Bún Cá Châu Đốc Dì Tư는 메콩 델타의 정통 맛을 선사합니다.', N'ヴィンカン通り75番地のBún Cá Châu Đốc Dì Tưは本格的なメコンデルタの味を提供します。'),
(N'Ốc Thảo 123', N'Hải sản', 35, 10.760750, 106.704600, 'ocoanh.jpg', N'Ốc Thảo tại số 123 Vĩnh Khánh là thiên đường hải sản đa dạng nhất phố.', N'Oc Thao at 123 Vinh Khanh is the most diverse seafood paradise on the street.', N'位于永康123号的Ốc Thảo是该街道最多样化的海鲜天堂。', N'빈칸 123번지의 옥 타오는 거리에서 가장 다양한 해산물 천국입니다.', N'ヴィンカン通り123番地のOc Thaoは通りで最も多様な海鮮の楽園です。'),
(N'Sushi KO', N'Đồ Nhật', 30, 10.760739, 106.704651, 'sushi.jpg', N'Sushi KO tại số 122 Vĩnh Khánh phục vụ các món Nhật Bản sáng tạo với giá bình dân.', N'Sushi KO at 122 Vinh Khanh serves creative Japanese dishes at affordable prices.', N'位于永康122号的Sushi KO以实惠的价格提供创意日本料理。', N'빈칸 122번지의 스시 KO는 저렴한 가격에 창의적인 일본 요리를 제공합니다.', N'ヴィンカン通り122番地のSushi KOはリーズナブルな価格で創作日本料理を提供しています。'),
(N'Chilli Lẩu Nướng 232', N'Lẩu nướng', 35, 10.760900, 106.703800, 'laubo.jpg', N'Chilli Lẩu Nướng tại số 232 Vĩnh Khánh phục vụ lẩu và nướng tự chọn giá từ 59.000đ.', N'Chilli Hot Pot & Grill at 232 Vinh Khanh serves all-you-can-choose hot pot and grilled dishes.', N'位于永康232号的Chilli火锅烧烤提供自选火锅和烧烤。', N'빈칸 232번지의 칠리 훠궈 & 그릴은 셀프 훠궈와 구이 요리를 제공합니다.', N'ヴィンカン通り232番地のChilli鍋&グリルはセルフ鍋と焼き料理を提供しています。'),
(N'Ốc Đào 2', N'Hải sản', 30, 10.760820, 106.703500, 'ocvu.jpg', N'Ốc Đào 2 tại số 232/123 Vĩnh Khánh đặc biệt nổi tiếng với sốt trứng muối.', N'Oc Dao 2 at 232/123 Vinh Khanh is especially famous for its salted egg sauce.', N'位于永康232/123号的Ốc Đào 2尤其以咸蛋酱而闻名。', N'빈칸 232/123번지의 옥 다오 2는 소금 달걀 소스로 유명합니다.', N'ヴィンカン通り232/123番地のOc Dao 2は塩卵ソースで有名です。'),
(N'Ốc Thảo 383', N'Hải sản', 30, 10.760770, 106.703400, 'ocoanh.jpg', N'Ốc Thảo tại số 383 Vĩnh Khánh có không gian rộng rãi, hải sản tươi ngon mỗi ngày.', N'Oc Thao at 383 Vinh Khanh has a spacious setting with fresh seafood daily.', N'位于永康383号的Ốc Thảo空间宽敞，每日提供新鲜海鲜。', N'빈칸 383번지의 옥 타오는 넓은 공간과 매일 신선한 해산물을 제공합니다.', N'ヴィンカン通り383番地のOc Thaoは広いスペースで毎日新鮮な海鮮を提供します。'),
(N'Ốc Oanh 534', N'Hải sản', 40, 10.760719, 106.703297, 'ocoanh.jpg', N'Ốc Oanh tại số 534 Vĩnh Khánh được Michelin Bib Gourmand 2024 vinh danh.', N'Oc Oanh at 534 Vinh Khanh was awarded Michelin Bib Gourmand 2024.', N'位于永康534号的Ốc Oanh荣获2024年米其林必比登推介。', N'빈칸 534번지의 옥 오안은 2024 미슐랭 빕 구르망을 수상했습니다.', N'ヴィンカン通り534番地のOc Oanhは2024年ミシュランビブグルマンを受賞。');

-- Nạp 50 Audios (Kịch bản TTS)
INSERT INTO Audios (AudioName, [Description], FilePath, [Language], PoiId) VALUES 
(N'Chào mừng', N'Tiếng Việt', N'Chào mừng bạn đến với phố ẩm thực Vĩnh Khánh, thiên đường ăn uống sầm uất nhất Quận 4.', 'VN', 1),
(N'Welcome', N'English', N'Welcome to Vinh Khanh Food Street, the most vibrant food paradise in District 4.', 'EN', 1),
(N'欢迎', N'Chinese', N'欢迎来到永康美食街，这是第四区最繁华的美食天堂。', 'ZH', 1),
(N'환영', N'Korean', N'4군 최고의 음식 천국 빈칸 푸드 스트리트에 오신 것을 환영합니다.', 'KO', 1),
(N'ようこそ', N'Japanese', N'4区最大の食の楽園、ヴィンカン・フードストリートへようこそ。', 'JA', 1),
(N'Lẩu dê', N'Tiếng Việt', N'Dê Chung nổi tiếng với món lẩu dê thơm ngon đặc trưng, mở cửa từ chiều đến đêm khuya.', 'VN', 2),
(N'Goat Hotpot', N'English', N'De Chung is famous for its aromatic goat hotpot, open from afternoon until late night.', 'EN', 2),
(N'山羊火锅', N'Chinese', N'德忠以其香气铺鼻的山羊火锅而闻名，从下午营业至深夜。', 'ZH', 2),
(N'염소 전골', N'Korean', N'드충은 향긋한 염소 전골로 유명하며 오후부터 늦은 밤까지 영업합니다.', 'KO', 2),
(N'ヤギ鍋', N'Japanese', N'Dê Chungは香り豊かなヤギ鍋で有名で、午後から深夜まで営業しています。', 'JA', 2),
(N'Hải sản', N'Tiếng Việt', N'Ốc Vũ 37 là quán ốc chuẩn địa phương, nổi tiếng với món ốc len xào dừa.', 'VN', 3),
(N'Seafood', N'English', N'Oc Vu 37 is an authentic local restaurant, famous for stir-fried snails with coconut.', 'EN', 3),
(N'海鲜', N'Chinese', N'吴氏37号是正宗的当地餐厅，以椰子炒蜗牛而闻名。', 'ZH', 3),
(N'해산물', N'Korean', N'옥 부 37은 정통 로컬 식당으로 코코넛 볶음 달팽이 요리가 유명합니다.', 'KO', 3),
(N'海鮮', N'Japanese', N'Oc Vu 37は地元の本格的な店で、大理石のカタツムリのココナッツ炒めが有名です。', 'JA', 3),
(N'Bún cá', N'Tiếng Việt', N'Bún cá Châu Đốc Dì Tư mang hương vị miền Tây đặc trưng từ vùng sông nước.', 'VN', 4),
(N'Fish Noodle', N'English', N'Di Tu Fish Noodle brings authentic Mekong Delta flavors from the river region.', 'EN', 4),
(N'鱼粉', N'Chinese', N'迪思阿姨鱼粉带来来自水乡的正宗湄公河三角洲风味。', 'ZH', 4),
(N'생선 국수', N'Korean', N'디 뜨어 생선 국수는 메콩 델타 지역의 정통 맛을 선사합니다.', 'KO', 4),
(N'魚の麺', N'Japanese', N'Di Tu 魚の麺は、水辺の地域からの本格的なメコンデルタの味を届けます。', 'JA', 4),
(N'Ốc Thảo 123', N'Tiếng Việt', N'Ốc Thảo 123 là thiên đường hải sản đa dạng nhất phố ẩm thực này.', 'VN', 5),
(N'Seafood Paradise', N'English', N'Oc Thao 123 is the most diverse seafood paradise on this food street.', 'EN', 5),
(N'海鲜天堂', N'Chinese', N'奥草123是这条美食街上品种最丰富的海鲜天堂。', 'ZH', 5),
(N'해산물 천국', N'Korean', N'옥 타오 123은 이 음식 거리에서 가장 다양한 해산물 천국입니다.', 'KO', 5),
(N'海鮮の楽園', N'Japanese', N'Oc Thao 123は、この美食街で最も多様な海鮮の楽園です。', 'JA', 5),
(N'Sushi KO', N'Tiếng Việt', N'Sushi KO phục vụ các món Nhật Bản sáng tạo với mức giá cực kỳ bình dân.', 'VN', 6),
(N'Sushi KO', N'English', N'Sushi KO serves creative Japanese dishes at very affordable prices.', 'EN', 6),
(N'寿司KO', N'Chinese', N'Sushi KO 以非常实惠的价格提供创意的日本料理。', 'ZH', 6),
(N'스시 KO', N'Korean', N'스시 KO는 매우 저렴한 가격에 창의적인 일본 요리를 제공합니다.', 'KO', 6),
(N'Sushi KO', N'Japanese', N'Sushi KOは、非常にリーズナブルな価格で創作日本料理を提供しています。', 'JA', 6),
(N'Chilli', N'Tiếng Việt', N'Chilli Lẩu Nướng phục vụ thực đơn tự chọn đa dạng, giá chỉ từ 59 nghìn đồng.', 'VN', 7),
(N'Chilli Grill', N'English', N'Chilli Hotpot and Grill serves a diverse buffet, starting from only 59,000 VND.', 'EN', 7),
(N'红辣椒烧烤', N'Chinese', N'红辣椒火锅烧烤提供多样的自助餐，价格仅从59,000越南盾起。', 'ZH', 7),
(N'칠리 그릴', N'Korean', N'칠리 훠궈와 그릴은 59,000동부터 시작하는 다양한 뷔페를 제공합니다.', 'KO', 7),
(N'Chilliグリル', N'Japanese', N'Chilli鍋とグリルは、わずか59,000ドンからの多様なビュッフェを提供しています。', 'JA', 7),
(N'Ốc Đào 2', N'Tiếng Việt', N'Ốc Đào 2 đặc biệt nổi tiếng với công thức sốt trứng muối độc quyền thơm ngậy.', 'VN', 8),
(N'Oc Dao 2', N'English', N'Oc Dao 2 is especially famous for its exclusive and creamy salted egg sauce recipe.', 'EN', 8),
(N'奥道2号', N'Chinese', N'奥道2号因其独家且浓郁的咸蛋酱配方而特别闻名。', 'ZH', 8),
(N'옥 다오 2', N'Korean', N'옥 다오 2는 독점적이고 부드러운 소금 달걀 소스 레시피로 특히 유명합니다.', 'KO', 8),
(N'Oc Dao 2', N'Japanese', N'Oc Dao 2は、独占的でクリーミーな塩卵ソースのレシピで特に有名です。', 'JA', 8),
(N'Ốc Thảo 383', N'Tiếng Việt', N'Ốc Thảo 383 có không gian rộng rãi, phù hợp cho những bữa tiệc hải sản đông người.', 'VN', 9),
(N'Oc Thao 383', N'English', N'Oc Thao 383 has a spacious setting, suitable for large seafood parties.', 'EN', 9),
(N'奥草383', N'Chinese', N'奥草383空间宽敞，适合举行大型海鲜派对。', 'ZH', 9),
(N'옥 타오 383', N'Korean', N'옥 타오 383은 넓은 공간을 갖추고 있어 대규모 해산물 파티에 적합합니다.', 'KO', 9),
(N'Oc Thao 383', N'Japanese', N'Oc Thao 383は広い空間があり、大人数での海鮮パーティーに適しています。', 'JA', 9),
(N'Ốc Oanh', N'Tiếng Việt', N'Ốc Oanh vinh dự nhận giải thưởng Michelin Bib Gourmand cho chất lượng hải sản tuyệt vời.', 'VN', 10),
(N'Oc Oanh', N'English', N'Oc Oanh is honored to receive the Michelin Bib Gourmand award for excellent seafood quality.', 'EN', 10),
(N'奥安海鲜', N'Chinese', N'奥安荣获米其林必比登推介，以表彰其卓越的海鲜品质。', 'ZH', 10),
(N'옥 오안', N'Korean', N'옥 오안은 뛰어난 해산물 품질로 미슐랭 빕 구르망상을 수상한 영광을 안았습니다.', 'KO', 10),
(N'Oc Oanh', N'Japanese', N'Oc Oanhは、優れた海鮮の品質でミシュラン・ビブグルマンを受賞したことを光栄に思います。', 'JA', 10);

-- Nạp 3 Hành trình (Tours)
INSERT INTO Tours (TourName, [Description], TotalTime) VALUES 
(N'Tour Ốc Michelin & Đặc Sản Quận 4', N'Hành trình thưởng thức những quán ốc được Michelin vinh danh và các quán ốc lâu đời nhất phố Vĩnh Khánh.', N'2 Giờ 30 Phút'),
(N'Lộ Trình Ăn Đêm Sầm Uất', N'Dạo quanh các điểm ăn uống nhộn nhịp từ đầu phố đến cuối phố, bao gồm cả Lẩu và Đồ nướng.', N'3 Giờ'),
(N'Tour Hải Sản Đa Quốc Gia', N'Trải nghiệm sự giao thoa văn hóa ẩm thực giữa Hải sản Việt Nam và Sushi Nhật Bản.', N'2 Giờ');

-- Nạp Chi tiết hành trình (TourDetails)
INSERT INTO TourDetails (TourId, PoiId, [Order]) VALUES 
(1, 1, 1), (1, 3, 2), (1, 8, 3), (1, 10, 4),
(2, 2, 1), (2, 4, 2), (2, 7, 3), (2, 9, 4),
(3, 1, 1), (3, 5, 2), (3, 6, 3);

-- Nạp Nhật ký truy cập (ActivityLogs)
INSERT INTO ActivityLogs (PoiId, ActionType, LanguageUsed, DeviceType) VALUES 
(1, 'AutoTrigger', 'VN', 'iOS'), (1, 'AutoTrigger', 'EN', 'Android'), (1, 'AutoTrigger', 'ZH', 'iOS'),
(2, 'ScanQR', 'VN', 'Android'), (2, 'AutoTrigger', 'VN', 'iOS'),
(3, 'ScanQR', 'VN', 'Android'), (3, 'ScanQR', 'EN', 'iOS'), (3, 'ScanQR', 'KO', 'Android'), (3, 'AutoTrigger', 'VN', 'iOS'),
(4, 'ScanQR', 'VN', 'Android'),
(5, 'AutoTrigger', 'VN', 'iOS'), (5, 'ScanQR', 'EN', 'Android'),
(6, 'ScanQR', 'KO', 'iOS'), (6, 'ScanQR', 'KO', 'Android'), (6, 'ScanQR', 'ZH', 'iOS'), (6, 'AutoTrigger', 'EN', 'Android'),
(7, 'ScanQR', 'VN', 'iOS'), (7, 'AutoTrigger', 'VN', 'Android'),
(8, 'ScanQR', 'VN', 'Android'), (8, 'ScanQR', 'VN', 'iOS'), (8, 'AutoTrigger', 'EN', 'Android'),
(9, 'ScanQR', 'VN', 'iOS'), (9, 'AutoTrigger', 'FR', 'iOS'),
(10, 'ScanQR', 'VN', 'Android'), (10, 'ScanQR', 'VN', 'iOS'), (10, 'ScanQR', 'EN', 'iOS'), 
(10, 'AutoTrigger', 'EN', 'Android'), (10, 'ScanQR', 'KO', 'iOS'), (10, 'AutoTrigger', 'VN', 'Android'), (10, 'ScanQR', 'ZH', 'iOS');
GO