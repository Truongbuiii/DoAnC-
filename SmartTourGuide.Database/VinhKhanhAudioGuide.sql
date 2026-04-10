USE VinhKhanhAudioGuide;
GO

-- ==========================================
-- 1. DỌN DẸP HỆ THỐNG
-- ==========================================
DROP TABLE IF EXISTS ActivityLogs;
DROP TABLE IF EXISTS TourDetails;
DROP TABLE IF EXISTS Audios;
DROP TABLE IF EXISTS Tours;
DROP TABLE IF EXISTS MenuItems;
DROP TABLE IF EXISTS POIs;
DROP TABLE IF EXISTS Admins;
GO

-- ==========================================
-- 2. TẠO CẤU TRÚC BẢNG (1 NGÔN NGỮ GỐC)
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
    Script NVARCHAR(MAX),       
    AudioFilePath NVARCHAR(MAX), 
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

-- ==========================================
-- 3. NẠP DỮ LIỆU
-- ==========================================
-- 3.1 Nạp Admins
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

-- 3.2 Nạp POIs
INSERT INTO POIs (Name, Category, TriggerRadius, Latitude, Longitude, ImageSource, DescriptionVi, OwnerUsername) VALUES 
(N'Cổng chào Phố ẩm thực', N'Điểm tham quan', 50, 10.761858, 106.702236, 'cong_chao.jpg', N'Chào mừng bạn đến với phố ẩm thực Vĩnh Khánh, thiên đường ăn uống về đêm sầm uất nhất Quận 4. Đây là điểm bắt đầu cho hành trình khám phá vị giác của bạn.', 'congchao'),
(N'Dê Chung', N'Lẩu dê', 35, 10.761500, 106.702600, 'laubo.jpg', N'Dê Chung tại số 3 Vĩnh Khánh nổi tiếng với món lẩu dê thơm ngon đặc trưng, nước dùng đậm đà nấu cùng thảo mộc thanh mát.', 'laudechung'),
(N'Ốc Vũ 37', N'Hải sản', 35, 10.761403, 106.702705, 'ocvu.jpg', N'Ốc Vũ là quán ốc chuẩn local của Quận 4 với phong cách chế biến dân dã nhưng đậm đà. Món ăn làm nên thương hiệu của quán là ốc len xào dừa.', 'ocvu'),
(N'Bún cá Châu Đốc Dì Tư', N'Bún cá', 30, 10.761200, 106.703100, 'bunmam.jpg', N'Bún cá Dì Tư mang trọn hương vị miền Tây sông nước lên phố thị. Miếng cá lóc đồng chắc thịt, nước lèo vàng ươm màu nghệ.', 'ditubunca'),
(N'Ốc Thảo 123', N'Hải sản', 35, 10.760750, 106.704600, 'octhao123.jpg', N'Ốc Thảo 123 là thiên đường hải sản sầm uất bậc nhất con phố. Với thực đơn đa dạng hàng trăm món ốc và hải sản tươi sống.', 'octhao123'),
(N'Sushi KO', N'Đồ Nhật', 30, 10.760739, 106.704651, 'sushi.jpg', N'Sushi KO mang đến làn gió mới cho phố Vĩnh Khánh với các món Nhật Bản sáng tạo. Với tiêu chí chất lượng Nhật - giá bình dân.', 'sushiko'),
(N'Chilli Lẩu Nướng 232', N'Lẩu nướng', 35, 10.760900, 106.703800, 'launuong.jpg', N'Chilli phục vụ thực đơn lẩu và nướng tự chọn phong phú với giá cả phải chăng. Đặc trưng của quán là các loại nước sốt ướp thịt đậm đà.', 'chilli'),
(N'Ốc Đào 2', N'Hải sản', 30, 10.760820, 106.703500, 'ocdao.jpg', N'Ốc Đào 2 là thương hiệu hải sản lừng lẫy Sài Gòn nay đã có mặt tại Vĩnh Khánh. Quán đặc biệt hút khách nhờ công thức sốt trứng muối.', 'ocdao'),
(N'Ốc Thảo 383', N'Hải sản', 30, 10.760770, 106.703400, 'octhao383.jpg', N'Ốc Thảo 383 sở hữu không gian vô cùng rộng rãi và thoáng đãng, cực kỳ phù hợp cho các buổi tiệc đoàn đông người.', 'octhao383'),
(N'Ốc Oanh 534', N'Hải sản', 40, 10.760719, 106.703297, 'ocoanh.jpg', N'Ốc Oanh là niềm tự hào của phố ẩm thực khi được Michelin Bib Gourmand 2024 vinh danh. Những con ốc kích cỡ khủng, nước sốt đậm đà.', 'ochoanh');

-- 3.3 Nạp MenuItems
INSERT INTO MenuItems(PoiId, DishName, Price, ImageSource, IsRecommended) VALUES
(1, N'Bánh tráng nướng', '25.000đ', 'banh_trang.jpg', 1), (1, N'Trà dâu tằm', '20.000đ', 'tra_dau.jpg', 1),
(2, N'Lẩu dê gia truyền', '250.000đ', 'lau_de.jpg', 1), (2, N'Dê nướng tảng', '180.000đ', 'de_nuong.jpg', 1), (2, N'Vú dê nướng chao', '150.000đ', 'vu_de.jpg', 0),
(3, N'Ốc len xào dừa', '60.000đ', 'oc_len.jpg', 1), (3, N'Càng ghẹ rang muối tuyết', '120.000đ', 'cang_ghe.jpg', 1), (3, N'Sò lông nướng mỡ hành', '50.000đ', 'so_long.jpg', 0),
(4, N'Bún cá Châu Đốc đặc biệt', '45.000đ', 'bun_ca.jpg', 1), (4, N'Bún mắm cốt miền Tây', '55.000đ', 'bun_mam.jpg', 1), (4, N'Đầu cá lóc hấp', '40.000đ', 'dau_ca.jpg', 0),
(5, N'Ốc hương rang muối', '90.000đ', 'oc_huong.jpg', 1), (5, N'Hàu nướng phô mai Pháp', '35.000đ/con', 'hau_phomai.jpg', 1), (5, N'Cháo hải sản nồi đất', '80.000đ', 'chao_haisan.jpg', 0),
(6, N'Sashimi cá hồi tươi', '120.000đ', 'sashimi.jpg', 1), (6, N'Cơm cuộn lươn Nhật', '95.000đ', 'sushi_luon.jpg', 1), (6, N'Mỳ Udon hải sản', '75.000đ', 'udon.jpg', 0),
(7, N'Lẩu Thái chua cay', '199.000đ', 'lau_thai.jpg', 1), (7, N'Ba chỉ bò Mỹ sốt Chilli', '89.000đ', 'bo_nuong.jpg', 1), (7, N'Tôm càng nướng mọi', '150.000đ', 'tom_nuong.jpg', 0),
(8, N'Ốc móng tay xào rau muống', '70.000đ', 'oc_mongtay.jpg', 1), (8, N'Ốc dừa xào bơ cay', '65.000đ', 'oc_dua.jpg', 1), (8, N'Răng mực xào bơ tỏi', '80.000đ', 'rang_muc.jpg', 0),
(9, N'Nghêu hấp sả', '55.000đ', 'ngheu_hap.jpg', 1), (9, N'Mỳ xào ốc móng tay', '75.000đ', 'my_xao.jpg', 1), (9, N'Cơm chiên hải sản', '85.000đ', 'com_chien.jpg', 0),
(10, N'Ốc hương sốt trứng muối', '150.000đ', 'oc_huong_tm.jpg', 1), (10, N'Càng cúm núm rang muối', '110.000đ', 'cang_cum.jpg', 1), (10, N'Ốc tỏi nướng mỡ hành', '55.000đ/con', 'oc_toi.jpg', 0);

-- 3.4 Nạp Audios
CREATE TABLE #TempAudio (AName NVARCHAR(200), ADesc NVARCHAR(200), AScript NVARCHAR(MAX), APath NVARCHAR(MAX), SearchName NVARCHAR(100));
INSERT INTO #TempAudio VALUES
(N'Chào mừng', N'Kịch bản tiếng Việt', N'Chào mừng bạn đã đến với khu phố ẩm thực nhộn nhịp nhất Sài Gòn. Hãy đi dọc theo con phố để khám phá nhé!', NULL, N'%Cổng chào%'),
(N'Lẩu dê', N'Kịch bản tiếng Việt', N'Đừng bỏ qua món lẩu dê trứ danh tại Dê Chung. Nước dùng được hầm kỹ với thảo mộc, cực kỳ bổ dưỡng.', NULL, N'%Dê Chung%'),
(N'Hải sản', N'Kịch bản tiếng Việt', N'Ốc Vũ 37 là điểm đến tuyệt vời cho các món ốc xào me, xào tỏi. Nhất định phải thử ốc len xào dừa nhé.', NULL, N'%Ốc Vũ%'),
(N'Bún cá', N'Kịch bản tiếng Việt', N'Hương vị miền Tây thu nhỏ tại Bún cá Dì Tư. Tô bún nóng hổi, cá lóc ngọt thịt sẽ làm ấm bụng bạn.', NULL, N'%Dì Tư%'),
(N'Ốc Thảo 123', N'Kịch bản tiếng Việt', N'Một trong những quán ốc đông đúc nhất khu vực. Hải sản tươi sống, chế biến nhanh và cực kỳ đậm đà.', NULL, N'%Ốc Thảo 123%'),
(N'Sushi KO', N'Kịch bản tiếng Việt', N'Trải nghiệm ẩm thực Nhật Bản ngay giữa lòng phố ốc. Sushi và sashimi ở đây luôn tươi ngon mỗi ngày.', NULL, N'%Sushi KO%'),
(N'Chilli', N'Kịch bản tiếng Việt', N'Hương vị lẩu nướng cay nồng đúng điệu. Thích hợp cho những buổi tiệc nướng cùng gia đình và bạn bè.', NULL, N'%Chilli%'),
(N'Ốc Đào 2', N'Kịch bản tiếng Việt', N'Chi nhánh của thương hiệu Ốc Đào nổi tiếng. Hãy thử ngay càng ghẹ rang muối và ốc hương xào bơ.', NULL, N'%Ốc Đào%'),
(N'Ốc Thảo 383', N'Kịch bản tiếng Việt', N'Không gian rộng rãi, thoáng mát. Thực đơn đa dạng với các món ốc và hải sản theo mùa.', NULL, N'%Ốc Thảo 383%'),
(N'Ốc Oanh', N'Kịch bản tiếng Việt', N'Quán ốc được cẩm nang Michelin giới thiệu. Nổi bật với các loại ốc "khổng lồ" và nước sốt bí truyền.', NULL, N'%Ốc Oanh%');

INSERT INTO Audios (AudioName, [Description], Script, AudioFilePath, PoiId)
SELECT T.AName, T.ADesc, T.AScript, T.APath, P.PoiId FROM #TempAudio T JOIN POIs P ON P.Name LIKE T.SearchName;
DROP TABLE #TempAudio;

-- 3.5 Nạp Tours & TourDetails
INSERT INTO Tours (TourName, [Description], TotalTime, ImageSource) VALUES
(N'Tour Ốc Michelin', N'Hành trình thưởng thức các quán ốc Michelin Quận 4.', N'2 Giờ 30 Phút', 'tour_oc.jpg'),
(N'Lộ Trình Ăn Đêm', N'Dạo quanh các điểm ăn đêm nhộn nhịp.', N'3 Giờ', 'tour_andem.jpg'),
(N'Tour Hải Sản Đa Quốc Gia', N'Giao thoa hải sản Việt và Sushi Nhật.', N'2 Giờ', 'tour_haisan.jpg');

INSERT INTO TourDetails (TourId, PoiId, [Order]) VALUES 
(1, 1, 1), (1, 3, 2), (1, 8, 3), (1, 10, 4),
(2, 2, 1), (2, 4, 2), (2, 7, 3), (2, 9, 4),
(3, 1, 1), (3, 5, 2), (3, 6, 3);

-- 3.6 Nạp Logs
INSERT INTO ActivityLogs (PoiId, ActionType, LanguageUsed, DeviceType, AccessTime) VALUES 
(1, N'Listen', 'VI', 'iPhone 15', GETDATE()), (10, N'Listen', 'EN', 'Android', GETDATE());
GO

-- ==========================================
-- 4. KIỂM TRA LẠI (10 Audios gốc, 10 POIs)
-- ==========================================
SELECT COUNT(*) AS [So Luong Audio] FROM Audios;
SELECT COUNT(*) AS [So Luong Quan An] FROM POIs;