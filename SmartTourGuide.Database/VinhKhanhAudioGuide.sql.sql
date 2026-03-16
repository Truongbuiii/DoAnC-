-- 1. Tạo Database
CREATE DATABASE VinhKhanhAudioGuide;
GO
USE VinhKhanhAudioGuide;
GO

-- 2. Bảng Admins: Quản lý đăng nhập Web CMS
CREATE TABLE Admins (
    AdminId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) UNIQUE NOT NULL,
    Password NVARCHAR(100) NOT NULL,
    FullName NVARCHAR(100)
);

-- 3. Bảng Categories: Phân loại quán ăn
CREATE TABLE Categories (
    CategoryId INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL
);

-- 4. Bảng Locations: Quản lý tọa độ thực tế (Tách riêng để tối ưu)
CREATE TABLE Locations (
    LocationId INT PRIMARY KEY IDENTITY(1,1),
    Latitude FLOAT NOT NULL,             -- Vĩ độ
    Longitude FLOAT NOT NULL,            -- Kinh độ
    Address NVARCHAR(255),               -- Địa chỉ số nhà
    TriggerRadius FLOAT DEFAULT 30.0     -- Bán kính kích hoạt TTS (mét)
);

-- 5. Bảng POIs: Nội dung điểm đến (Cột Description là kịch bản cho máy đọc)
CREATE TABLE POIs (
    PoiId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),           -- Nội dung TTS sẽ đọc
    ImageSource NVARCHAR(255),           -- Tên file ảnh trong App
    CategoryId INT,
    LocationId INT UNIQUE,               -- Quan hệ 1-1 với Location
    FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId),
    FOREIGN KEY (LocationId) REFERENCES Locations(LocationId)
);

----------------------------------------------------------
-- 6. DỮ LIỆU MẪU THỰC TẾ CHI TIẾT (PHỐ VĨNH KHÁNH)
----------------------------------------------------------

-- Thêm Admin
INSERT INTO Admins (Username, Password, FullName) VALUES ('admin', '123456', N'Nguyễn Đức Tài');

-- Thêm Phân loại
INSERT INTO Categories (CategoryName) VALUES 
(N'Hải sản & Ốc'), (N'Món Lẩu'), (N'Ăn vặt & Đường phố'), (N'Giải khát'), (N'Đồ Nhật');

-- Thêm Tọa độ thực tế dọc phố Vĩnh Khánh (Dữ liệu phủ rộng)
INSERT INTO Locations (Latitude, Longitude, Address, TriggerRadius) VALUES 
(10.75750, 106.70700, N'Đầu phố Vĩnh Khánh', 50),  -- 1. Cổng chào
(10.75883, 106.70505, N'534 Vĩnh Khánh', 35),      -- 2. Ốc Oanh
(10.75916, 106.70452, N'37 Vĩnh Khánh', 30),       -- 3. Ốc Vũ
(10.75822, 106.70611, N'Khu Nhà Cháy', 40),        -- 4. Lẩu bò
(10.75850, 106.70555, N'200 Vĩnh Khánh', 25),      -- 5. Sữa tươi chiên
(10.75940, 106.70410, N'194 Vĩnh Khánh', 30),      -- 6. Phá lấu Dì Nũi
(10.75800, 106.70650, N'122 Vĩnh Khánh', 45),      -- 7. Sushi KO
(10.75860, 106.70580, N'135 Vĩnh Khánh', 30),      -- 8. Bún mắm 135
(10.75900, 106.70480, N'40 Vĩnh Khánh', 25),       -- 9. Trà sữa túi lọc
(10.75960, 106.70380, N'Hẻm 200 Vĩnh Khánh', 35);  -- 10. Xôi gà

-- Thêm Nội dung Thuyết minh tương ứng cho từng điểm (Dành cho TTS)
INSERT INTO POIs (Name, Description, ImageSource, CategoryId, LocationId) VALUES 
(N'Cổng chào Phố ẩm thực', N'Chào mừng bạn đến với phố ẩm thực Vĩnh Khánh, thiên đường ăn uống về đêm sầm uất nhất Quận 4. Con phố này nổi tiếng với các món hải sản và đồ nướng phong phú.', 'cong_chao.jpg', 3, 1),
(N'Ốc Oanh 534', N'Hệ thống nhận diện bạn đang đứng trước Ốc Oanh, quán ốc nổi tiếng nhất khu vực với món ốc hương sốt trứng muối đặc trưng. Đây là điểm dừng chân không thể bỏ qua.', 'ocoanh.jpg', 1, 2),
(N'Ốc Vũ 37', N'Bạn đang ở gần Ốc Vũ. Quán nổi tiếng với không gian rộng rãi và các loại hải sản tươi sống bắt tại hồ. Hãy thử món ốc len xào dừa khi ghé thăm quán nhé.', 'ocvu.jpg', 1, 3),
(N'Lẩu bò Khu Nhà Cháy', N'Bạn đang tiến vào khu vực quán Lẩu bò Nhà Cháy. Đây là địa điểm ăn uống lâu đời với nước dùng đậm đà, mang đậm hương vị truyền thống của vùng đất Quận 4.', 'laubo.jpg', 2, 4),
(N'Sữa tươi chiên Vĩnh Khánh', N'Hệ thống gợi ý bạn thử món sữa tươi chiên ngay phía trước. Những viên sữa được chiên vàng giòn bên ngoài, béo ngậy bên trong, rất thích hợp để ăn vặt.', 'suatuoi.jpg', 3, 5),
(N'Phá lấu Dì Nũi', N'Bạn đang ở gần hẻm phá lấu Dì Nũi nổi tiếng. Với hơn 20 năm kinh nghiệm, chén phá lấu ở đây béo ngậy nước cốt dừa, ăn kèm bánh mì giòn rất hấp dẫn.', 'phalau.jpg', 3, 6),
(N'Sushi KO', N'Phía trước bạn là Sushi KO. Một địa điểm thú vị với các món Nhật Bản giá bình dân nhưng chất lượng rất tươi ngon, thu hút rất đông các bạn trẻ mỗi tối.', 'sushi.jpg', 5, 7),
(N'Bún mắm 135', N'Chào mừng bạn đến với Bún mắm 135. Tô bún ở đây đầy đặn với tôm, mực, heo quay, kết hợp cùng các loại rau đặc trưng miền Tây tạo nên hương vị khó quên.', 'bunmam.jpg', 1, 8),
(N'Trà sữa túi lọc', N'Bạn đã đi gần đến khu vực giải khát. Hãy thưởng thức một ly trà sữa túi lọc truyền thống để làm dịu cơn khát sau khi dạo quanh các quán ăn cay nồng.', 'trasua.jpg', 4, 9),
(N'Xôi gà hẻm 200', N'Nếu bạn muốn đổi vị, hãy thử xôi gà tại hẻm 200. Xôi ở đây dẻo thơm, gà được chiên giòn rụm và tẩm ướp đậm đà, là lựa chọn tuyệt vời cho bữa ăn đêm.', 'xoiga.jpg', 3, 10);