-- 1. Tạo Database

CREATE DATABASE VinhKhanhAudioGuide;

GO

USE VinhKhanhAudioGuide;

GO



-- 2. Bảng Danh mục (Phân loại: Quán ốc, Quán lẩu, Ăn vặt, Cafe...)

CREATE TABLE Categories (

    CategoryId INT PRIMARY KEY IDENTITY(1,1),

    CategoryName NVARCHAR(100) NOT NULL

);



-- 3. Bảng POI (Điểm đến - Đây là bảng quan trọng nhất)

CREATE TABLE POIs (

    PoiId INT PRIMARY KEY IDENTITY(1,1),

    Name NVARCHAR(200) NOT NULL,

    Latitude FLOAT NOT NULL,             -- Vĩ độ

    Longitude FLOAT NOT NULL,            -- Kinh độ

    TriggerRadius FLOAT DEFAULT 20.0,    -- Bán kính kích hoạt (mét)

    AudioFileName NVARCHAR(255),         -- Tên file âm thanh thuyết minh

    ImageSource NVARCHAR(255),           -- Tên file ảnh hiển thị

    Description NVARCHAR(MAX),           -- Nội dung thuyết minh dạng chữ

    CategoryId INT,

    FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId)

);



-- 4. Bảng Lịch sử người dùng (Lưu lại những nơi Tài đã đi qua)

CREATE TABLE TravelHistory (

    HistoryId INT PRIMARY KEY IDENTITY(1,1),

    PoiId INT,

    VisitTime DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (PoiId) REFERENCES POIs(PoiId)

); -- Thêm danh mục

INSERT INTO Categories (CategoryName) VALUES (N'Quán Ốc'), (N'Phá Lấu'), (N'Sủi Cảo');



-- Thêm các quán ăn nổi tiếng Vĩnh Khánh (Tọa độ giả định, Tài sửa lại cho đúng chỗ bạn đứng nhé)

INSERT INTO POIs (Name, Latitude, Longitude, TriggerRadius, ImageSource, AudioFileName, Description, CategoryId)

VALUES 

(N'Ốc Oanh', 10.7677, 106.6836, 30, 'ocoanh.jpg', 'ocoanh_audio.mp3', N'Quán ốc nổi tiếng nhất phố Vĩnh Khánh...', 1),

(N'Ốc Vũ', 10.7678, 106.6838, 30, 'ocvu.jpg', 'ocvu_audio.mp3', N'Nổi tiếng với các món ốc xào bơ...', 1),

(N'Phá Lấu Dì Nũi', 10.7679, 106.6840, 20, 'phalau.jpg', 'phalau_audio.mp3', N'Phá lấu gia truyền hơn 20 năm...', 2);