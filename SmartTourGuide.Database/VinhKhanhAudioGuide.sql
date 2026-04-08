USE VinhKhanhAudioGuide;
GO

-- ==========================================
-- 1. DỌN DẸP HỆ THỐNG
-- ==========================================
DROP TABLE IF EXISTS ActivityLogs;
DROP TABLE IF EXISTS TourDetails;
DROP TABLE IF EXISTS Audios;
DROP TABLE IF EXISTS Tours;
DROP TABLE IF EXISTS MenuItems; -- Đã sửa tên đồng nhất
DROP TABLE IF EXISTS POIs;
DROP TABLE IF EXISTS Admins;
GO

-- ==========================================
-- 2. TẠO CẤU TRÚC BẢNG
-- ==========================================

CREATE TABLE Admins (
    AdminId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100),
    [Role] NVARCHAR(20) DEFAULT 'Owner'
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
    DescriptionJa NVARCHAR(MAX),
    OwnerUsername NVARCHAR(50) CONSTRAINT FK_POIs_Admins 
        FOREIGN KEY REFERENCES Admins(Username) 
        ON DELETE SET NULL 
        ON UPDATE CASCADE
);

CREATE TABLE MenuItems (
    MenuId INT PRIMARY KEY IDENTITY(1,1),
    PoiId INT FOREIGN KEY REFERENCES POIs(PoiId) ON DELETE CASCADE,
    DishName NVARCHAR(255),
    Price NVARCHAR(50),
    ImageSource NVARCHAR(MAX),
    IsRecommended BIT DEFAULT 1
);

CREATE TABLE Audios (
    AudioId INT PRIMARY KEY IDENTITY(1,1),
    AudioName NVARCHAR(255),
    [Description] NVARCHAR(MAX),
    FilePath NVARCHAR(MAX),
    [Language] NVARCHAR(10), 
    PoiId INT FOREIGN KEY REFERENCES POIs(PoiId) ON DELETE CASCADE
);

CREATE TABLE Tours (
    TourId INT PRIMARY KEY IDENTITY(1,1),
    TourName NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(MAX),
    TotalTime NVARCHAR(50),
    ImageSource NVARCHAR(MAX)
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

-- 3. NẠP 11 TÀI KHOẢN ADMINS
INSERT INTO Admins (Username, [Password], FullName, [Role]) VALUES 
('admin', '123456', N'2T', 'Admin'), 
('ochoanh', '123', N'Chủ quán Ốc Oanh 534', 'Owner'),
('ocvu', '123', N'Chủ quán Ốc Vũ 37', 'Owner'), 
('ocdao', '123', N'Chủ quán Ốc Đào 2', 'Owner'),
('octhao123', '123', N'Chủ quán Ốc Thảo 123', 'Owner'), 
('octhao383', '123', N'Chủ quán Ốc Thảo 383', 'Owner'),
('sushiko', '123', N'Chủ quán Sushi KO', 'Owner'), 
('chilli', '123', N'Chủ quán Chilli Lẩu Nướng', 'Owner'),
('laudechung', '123', N'Chủ quán Lẩu Dê Chung', 'Owner'), 
('ditubunca', '123', N'Chủ quán Bún cá Dì Tư', 'Owner'),
('congchao', '123', N'Ban Quản Lý Cổng Chào', 'Owner');

-- 4. NẠP 10 ĐỊA ĐIỂM POIS (Giữ nguyên tọa độ và mô tả của Duy)
INSERT INTO POIs (Name, Category, TriggerRadius, Latitude, Longitude, ImageSource, DescriptionVi, DescriptionEn, DescriptionZh, DescriptionKo, DescriptionJa, OwnerUsername) VALUES 
-- 1. Cổng chào
(N'Cổng chào Phố ẩm thực', N'Điểm tham quan', 50, 10.761858, 106.702236, 'cong_chao.jpg', 
N'Chào mừng bạn đến với phố ẩm thực Vĩnh Khánh, thiên đường ăn uống về đêm sầm uất nhất Quận 4, từng lọt top 10 con phố thú vị nhất thế giới 2025. Đây là điểm bắt đầu cho hành trình khám phá vị giác của bạn với hàng trăm quán ăn đa dạng.', 
N'Welcome to Vinh Khanh Food Street, the most vibrant nighttime food paradise in District 4, ranked top 10 most exciting streets in the world 2025. This is the starting point for your taste discovery journey.', 
NULL, NULL, NULL, 'congchao'),

-- 2. Dê Chung
(N'Dê Chung', N'Lẩu dê', 35, 10.761500, 106.702600, 'laubo.jpg', 
N'Dê Chung tại số 3 Vĩnh Khánh nổi tiếng với món lẩu dê thơm ngon đặc trưng, nước dùng đậm đà nấu cùng thảo mộc thanh mát. Quán có không gian mở thoáng mát, phục vụ từ chiều đến tận đêm khuya cho các tín đồ mê đặc sản dê.', 
N'De Chung at 3 Vinh Khanh is famous for its aromatic goat hotpot with herbal broth. The restaurant offers a breezy open space, serving from afternoon until late at night for goat specialty lovers.', 
NULL, NULL, NULL, 'laudechung'),

-- 3. Ốc Vũ 37
(N'Ốc Vũ 37', N'Hải sản', 35, 10.761403, 106.702705, 'ocvu.jpg', 
N'Ốc Vũ là quán ốc chuẩn local của Quận 4 với phong cách chế biến dân dã nhưng đậm đà. Món ăn làm nên thương hiệu của quán là ốc len xào dừa với nước sốt béo ngậy, thơm lừng cốt dừa tươi, ăn kèm bánh mì nóng giòn rất tuyệt.', 
N'Oc Vu at 37 Vinh Khanh is an authentic local seafood restaurant in District 4. Its signature dish is stir-fried snails with rich coconut sauce, perfectly paired with crispy bread.', 
NULL, NULL, NULL, 'ocvu'),

-- 4. Bún cá Châu Đốc Dì Tư
(N'Bún cá Châu Đốc Dì Tư', N'Bún cá', 30, 10.761200, 106.703100, 'bunmam.jpg', 
N'Bún cá Dì Tư mang trọn hương vị miền Tây sông nước lên phố thị. Miếng cá lóc đồng chắc thịt, nước lèo vàng ươm màu nghệ hòa quyện cùng rau nhút, bông điên điển tươi ngon tạo nên trải nghiệm ẩm thực miền Tây đúng điệu.', 
N'Bun Ca Chau Doc Di Tu at 75 Vinh Khanh brings authentic Mekong Delta flavors. Fresh snakehead fish and golden turmeric broth with local vegetables create a genuine Western Vietnamese experience.', 
NULL, NULL, NULL, 'ditubunca'),

-- 5. Ốc Thảo 123
(N'Ốc Thảo 123', N'Hải sản', 35, 10.760750, 106.704600, 'octhao123.jpg', 
N'Ốc Thảo 123 là thiên đường hải sản sầm uất bậc nhất con phố. Với thực đơn đa dạng hàng trăm món ốc và hải sản tươi sống được nhập mới mỗi ngày, quán luôn là lựa chọn hàng đầu cho các nhóm bạn muốn tận hưởng không khí nhộn nhịp.', 
N'Oc Thao at 123 Vinh Khanh is the most diverse seafood paradise on the street, offering hundreds of fresh dishes in a lively atmosphere for large groups of friends.', 
NULL, NULL, NULL, 'octhao123'),

-- 6. Sushi KO
(N'Sushi KO', N'Đồ Nhật', 30, 10.760739, 106.704651, 'sushi.jpg', 
N'Sushi KO mang đến làn gió mới cho phố Vĩnh Khánh với các món Nhật Bản sáng tạo. Với tiêu chí chất lượng Nhật - giá bình dân, đây là điểm đến lý tưởng nếu bạn muốn đổi vị với những miếng Sashimi tươi rói sau khi ăn hải sản.', 
N'Sushi KO serves creative Japanese dishes at affordable prices. It is a perfect spot to change your palate with fresh Sashimi after enjoying the local seafood dishes.', 
NULL, NULL, NULL, 'sushiko'),

-- 7. Chilli Lẩu Nướng 232
(N'Chilli Lẩu Nướng 232', N'Lẩu nướng', 35, 10.760900, 106.703800, 'launuong.jpg', 
N'Chilli phục vụ thực đơn lẩu và nướng tự chọn phong phú với giá cả phải chăng. Đặc trưng của quán là các loại nước sốt ướp thịt đậm đà, thơm cay đúng điệu và hệ thống bếp nướng hiện đại, thu hút rất đông các bạn trẻ tụ tập.', 
N'Chilli Hot Pot & Grill serves a diverse self-chosen menu. Famous for its flavorful and spicy marinades and modern grills, it is a highly popular gathering spot for local youth.', 
NULL, NULL, NULL, 'chilli'),

-- 8. Ốc Đào 2
(N'Ốc Đào 2', N'Hải sản', 30, 10.760820, 106.703500, 'ocdao.jpg', 
N'Ốc Đào 2 là thương hiệu hải sản lừng lẫy Sài Gòn nay đã có mặt tại Vĩnh Khánh. Quán đặc biệt hút khách nhờ công thức sốt trứng muối độc quyền thơm ngậy, không gian trang nhã sạch sẽ và cung cách phục vụ rất chuyên nghiệp.', 
N'Oc Dao 2 is a famous seafood brand in Saigon. The restaurant is particularly popular for its exclusive, creamy salted egg sauce and professional service in a clean environment.', 
NULL, NULL, NULL, 'ocdao'),

-- 9. Ốc Thảo 383
(N'Ốc Thảo 383', N'Hải sản', 30, 10.760770, 106.703400, 'octhao383.jpg', 
N'Ốc Thảo 383 sở hữu không gian vô cùng rộng rãi và thoáng đãng, cực kỳ phù hợp cho các buổi tiệc đoàn đông người. Hải sản ở đây luôn đảm bảo tiêu chí tươi - ngon - rẻ với cách chế biến đậm đà nịnh miệng thực khách.', 
N'Oc Thao at 383 Vinh Khanh features a very spacious and airy setting, ideal for large group parties. Seafood is always fresh and tasty at reasonable prices.', 
NULL, NULL, NULL, 'octhao383'),

-- 10. Ốc Oanh 534
(N'Ốc Oanh 534', N'Hải sản', 40, 10.760719, 106.703297, 'ocoanh.jpg', 
N'Ốc Oanh là niềm tự hào của phố ẩm thực khi được Michelin Bib Gourmand 2024 vinh danh. Những con ốc kích cỡ khủng, nước sốt đậm đà và không khí nhộn nhịp đặc trưng đã tạo nên thương hiệu không thể trộn lẫn của quán.', 
N'Oc Oanh was honored with the Michelin Bib Gourmand 2024. Famous for its oversized snails, bold seasonings, and vibrant atmosphere, it is a must-visit icon on the street.', 
NULL, NULL, NULL, 'ochoanh');
GO

-- 5. NẠP TOURS VÀ TOURDETAILS
INSERT INTO Tours (TourName, [Description], TotalTime, ImageSource) VALUES
(N'Tour Ốc Michelin', N'Hành trình thưởng thức các quán ốc Michelin Quận 4.', N'2 Giờ 30 Phút', 'tour_oc.jpg'),
(N'Lộ Trình Ăn Đêm', N'Dạo quanh các điểm ăn đêm nhộn nhịp.', N'3 Giờ', 'tour_andem.jpg'),
(N'Tour Hải Sản Đa Quốc Gia', N'Giao thoa hải sản Việt và Sushi Nhật.', N'2 Giờ', 'tour_haisan.jpg');

INSERT INTO TourDetails (TourId, PoiId, [Order]) VALUES 
(1, 1, 1), (1, 3, 2), (1, 8, 3), (1, 10, 4),
(2, 2, 1), (2, 4, 2), (2, 7, 3), (2, 9, 4),
(3, 1, 1), (3, 5, 2), (3, 6, 3);

-- 6. NẠP 50 AUDIOS QUA BẢNG TẠM (Sửa 'VN' thành 'VI')
CREATE TABLE #TempAudio (AName NVARCHAR(200), ADesc NVARCHAR(200), APath NVARCHAR(MAX), ALang NVARCHAR(10), SearchName NVARCHAR(100));

INSERT INTO #TempAudio VALUES
(N'Chào mừng', N'Tiếng Việt', N'audio/vi/welcome.mp3', 'VI', N'%Cổng chào%'), (N'Welcome', N'English', N'audio/en/welcome.mp3', 'EN', N'%Cổng chào%'), (N'欢迎', N'Chinese', N'audio/zh/welcome.mp3', 'ZH', N'%Cổng chào%'), (N'환영', N'Korean', N'audio/ko/welcome.mp3', 'KO', N'%Cổng chào%'), (N'ようこそ', N'Japanese', N'audio/ja/welcome.mp3', 'JA', N'%Cổng chào%'),
(N'Lẩu dê', N'Tiếng Việt', N'audio/vi/dechung.mp3', 'VI', N'%Dê Chung%'), (N'Goat Hotpot', N'English', N'audio/en/dechung.mp3', 'EN', N'%Dê Chung%'), (N'山羊火锅', N'Chinese', N'audio/zh/dechung.mp3', 'ZH', N'%Dê Chung%'), (N'염소 전골', N'Korean', N'audio/ko/dechung.mp3', 'KO', N'%Dê Chung%'), (N'ヤギ鍋', N'Japanese', N'audio/ja/dechung.mp3', 'JA', N'%Dê Chung%'),
(N'Hải sản', N'Tiếng Việt', N'audio/vi/ocvu.mp3', 'VI', N'%Ốc Vũ%'), (N'Seafood', N'English', N'audio/en/ocvu.mp3', 'EN', N'%Ốc Vũ%'), (N'海鲜', N'Chinese', N'audio/zh/ocvu.mp3', 'ZH', N'%Ốc Vũ%'), (N'해산물', N'Korean', N'audio/ko/ocvu.mp3', 'KO', N'%Ốc Vũ%'), (N'海鮮', N'Japanese', N'audio/ja/ocvu.mp3', 'JA', N'%Ốc Vũ%'),
(N'Bún cá', N'Tiếng Việt', N'audio/vi/ditubunca.mp3', 'VI', N'%Dì Tư%'), (N'Fish Noodle', N'English', N'audio/en/ditubunca.mp3', 'EN', N'%Dì Tư%'), (N'鱼粉', N'Chinese', N'audio/zh/ditubunca.mp3', 'ZH', N'%Dì Tư%'), (N'생선 국수', N'Korean', N'audio/ko/ditubunca.mp3', 'KO', N'%Dì Tư%'), (N'魚の麺', N'Japanese', N'audio/ja/ditubunca.mp3', 'JA', N'%Dì Tư%'),
(N'Ốc Thảo 123', N'Tiếng Việt', N'audio/vi/octhao123.mp3', 'VI', N'%Ốc Thảo 123%'), (N'Seafood Paradise', N'English', N'audio/en/octhao123.mp3', 'EN', N'%Ốc Thảo 123%'), (N'海鲜天堂', N'Chinese', N'audio/zh/octhao123.mp3', 'ZH', N'%Ốc Thảo 123%'), (N'해산물 천국', N'Korean', N'audio/ko/octhao123.mp3', 'KO', N'%Ốc Thảo 123%'), (N'海鮮の楽園', N'Japanese', N'audio/ja/octhao123.mp3', 'JA', N'%Ốc Thảo 123%'),
(N'Sushi KO', N'Tiếng Việt', N'audio/vi/sushiko.mp3', 'VI', N'%Sushi KO%'), (N'Sushi KO', N'English', N'audio/en/sushiko.mp3', 'EN', N'%Sushi KO%'), (N'寿司KO', N'Chinese', N'audio/zh/sushiko.mp3', 'ZH', N'%Sushi KO%'), (N'스시 KO', N'Korean', N'audio/ko/sushiko.mp3', 'KO', N'%Sushi KO%'), (N'Sushi KO', N'Japanese', N'audio/ja/sushiko.mp3', 'JA', N'%Sushi KO%'),
(N'Chilli', N'Tiếng Việt', N'audio/vi/chilli.mp3', 'VI', N'%Chilli%'), (N'Chilli Grill', N'English', N'audio/en/chilli.mp3', 'EN', N'%Chilli%'), (N'红辣椒烧烤', N'Chinese', N'audio/zh/chilli.mp3', 'ZH', N'%Chilli%'), (N'칠리 그릴', N'Korean', N'audio/ko/chilli.mp3', 'KO', N'%Chilli%'), (N'Chilliグリル', N'Japanese', N'audio/ja/chilli.mp3', 'JA', N'%Chilli%'),
(N'Ốc Đào 2', N'Tiếng Việt', N'audio/vi/ocdao.mp3', 'VI', N'%Ốc Đào%'), (N'Oc Dao 2', N'English', N'audio/en/ocdao.mp3', 'EN', N'%Ốc Đào%'), (N'奥道2号', N'Chinese', N'audio/zh/ocdao.mp3', 'ZH', N'%Ốc Đào%'), (N'옥 다오 2', N'Korean', N'audio/ko/ocdao.mp3', 'KO', N'%Ốc Đào%'), (N'Oc Dao 2', N'Japanese', N'audio/ja/ocdao.mp3', 'JA', N'%Ốc Đào%'),
(N'Ốc Thảo 383', N'Tiếng Việt', N'audio/vi/octhao383.mp3', 'VI', N'%Ốc Thảo 383%'), (N'Oc Thao 383', N'English', N'audio/en/octhao383.mp3', 'EN', N'%Ốc Thảo 383%'), (N'奥草383', N'Chinese', N'audio/zh/octhao383.mp3', 'ZH', N'%Ốc Thảo 383%'), (N'옥 타오 383', N'Korean', N'audio/ko/octhao383.mp3', 'KO', N'%Ốc Thảo 383%'), (N'Oc Thao 383', N'Japanese', N'audio/ja/octhao383.mp3', 'JA', N'%Ốc Thảo 383%'),
(N'Ốc Oanh', N'Tiếng Việt', N'audio/vi/ocoanh.mp3', 'VI', N'%Ốc Oanh%'), (N'Oc Oanh', N'English', N'audio/en/ocoanh.mp3', 'EN', N'%Ốc Oanh%'), (N'奥安海鲜', N'Chinese', N'audio/zh/ocoanh.mp3', 'ZH', N'%Ốc Oanh%'), (N'옥 오안', N'Korean', N'audio/ko/ocoanh.mp3', 'KO', N'%Ốc Oanh%'), (N'Oc Oanh', N'Japanese', N'audio/ja/ocoanh.mp3', 'JA', N'%Ốc Oanh%');


-- Nạp dữ liệu món ăn đặc trưng cho từng quán
INSERT INTO MenuItems(PoiId, DishName, Price, ImageSource, IsRecommended) VALUES
-- 1. Cổng chào (Thường bán đồ ăn vặt nhẹ xung quanh)
(1, N'Bánh tráng nướng', '25.000đ', 'banh_trang.jpg', 1),
(1, N'Trà dâu tằm', '20.000đ', 'tra_dau.jpg', 1),

-- 2. Dê Chung (Chuyên các món dê)
(2, N'Lẩu dê gia truyền', '250.000đ', 'lau_de.jpg', 1),
(2, N'Dê nướng tảng', '180.000đ', 'de_nuong.jpg', 1),
(2, N'Vú dê nướng chao', '150.000đ', 'vu_de.jpg', 0),

-- 3. Ốc Vũ 37 (Ốc local Quận 4)
(3, N'Ốc len xào dừa', '60.000đ', 'oc_len.jpg', 1),
(3, N'Càng ghẹ rang muối tuyết', '120.000đ', 'cang_ghe.jpg', 1),
(3, N'Sò lông nướng mỡ hành', '50.000đ', 'so_long.jpg', 0),

-- 4. Bún cá Châu Đốc Dì Tư (Đặc sản miền Tây)
(4, N'Bún cá Châu Đốc đặc biệt', '45.000đ', 'bun_ca.jpg', 1),
(4, N'Bún mắm cốt miền Tây', '55.000đ', 'bun_mam.jpg', 1),
(4, N'Đầu cá lóc hấp', '40.000đ', 'dau_ca.jpg', 0),

-- 5. Ốc Thảo 123 (Menu đa dạng)
(5, N'Ốc hương rang muối', '90.000đ', 'oc_huong.jpg', 1),
(5, N'Hàu nướng phô mai Pháp', '35.000đ/con', 'hau_phomai.jpg', 1),
(5, N'Cháo hải sản nồi đất', '80.000đ', 'chao_haisan.jpg', 0),

-- 6. Sushi KO (Đồ Nhật bình dân)
(6, N'Sashimi cá hồi tươi', '120.000đ', 'sashimi.jpg', 1),
(6, N'Cơm cuộn lươn Nhật', '95.000đ', 'sushi_luon.jpg', 1),
(6, N'Mỳ Udon hải sản', '75.000đ', 'udon.jpg', 0),

-- 7. Chilli Lẩu Nướng 232 (Buffet/Alacarte nướng)
(7, N'Lẩu Thái chua cay', '199.000đ', 'lau_thai.jpg', 1),
(7, N'Ba chỉ bò Mỹ sốt Chilli', '89.000đ', 'bo_nuong.jpg', 1),
(7, N'Tôm càng nướng mọi', '150.000đ', 'tom_nuong.jpg', 0),

-- 8. Ốc Đào 2 (Nổi tiếng sốt trứng muối)
(8, N'Ốc móng tay xào rau muống', '70.000đ', 'oc_mongtay.jpg', 1),
(8, N'Ốc dừa xào bơ cay', '65.000đ', 'oc_dua.jpg', 1),
(8, N'Răng mực xào bơ tỏi', '80.000đ', 'rang_muc.jpg', 0),

-- 9. Ốc Thảo 383 (Không gian tiệc tùng)
(9, N'Nghêu hấp sả', '55.000đ', 'ngheu_hap.jpg', 1),
(9, N'Mỳ xào ốc móng tay', '75.000đ', 'my_xao.jpg', 1),
(9, N'Cơm chiên hải sản', '85.000đ', 'com_chien.jpg', 0),

-- 10. Ốc Oanh 534 (Michelin Bib Gourmand)
(10, N'Ốc hương sốt trứng muối', '150.000đ', 'oc_huong_tm.jpg', 1),
(10, N'Càng cúm núm rang muối', '110.000đ', 'cang_cum.jpg', 1),
(10, N'Ốc tỏi nướng mỡ hành', '55.000đ/con', 'oc_toi.jpg', 0);
GO

INSERT INTO Audios (AudioName, [Description], FilePath, [Language], PoiId)
SELECT T.AName, T.ADesc, T.APath, T.ALang, P.PoiId FROM #TempAudio T JOIN POIs P ON P.Name LIKE T.SearchName;
DROP TABLE #TempAudio;
GO

-- 7. NẠP NHẬT KÝ MẪU (Đã chuẩn hóa VI)
INSERT INTO ActivityLogs (PoiId, ActionType, LanguageUsed, DeviceType, AccessTime) VALUES 
(1, N'Listen', 'VI', 'iPhone 15', GETDATE()), (10, N'Listen', 'EN', 'Android', GETDATE());
GO

-- Kiểm tra lại (Phải ra 50 Audios và 10 POIs)
SELECT COUNT(*) AS [So Luong Audio] FROM Audios;
SELECT COUNT(*) AS [So Luong Quan An] FROM POIs;