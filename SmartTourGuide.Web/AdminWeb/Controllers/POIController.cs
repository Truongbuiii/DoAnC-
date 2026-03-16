using AdminWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace AdminWeb.Controllers
{
    public class POIController : Controller
    {
        // Tạo một danh sách tĩnh để lưu dữ liệu tạm thời thay cho Database
        private static List<POI> _samplePois = new List<POI>();
        private static List<Category> _categories = new List<Category>();
        private static int _nextId = 1;

        // Hàm khởi tạo dữ liệu mẫu (Chạy 1 lần duy nhất khi bạn mở Web)
        static POIController()
        {
            // 1. Dữ liệu mẫu cho Phân loại
            _categories.Add(new Category { CategoryId = 1, CategoryName = "Hải sản & Ốc" });
            _categories.Add(new Category { CategoryId = 2, CategoryName = "Món Lẩu" });
            _categories.Add(new Category { CategoryId = 3, CategoryName = "Ăn vặt" });

            // 2. Dữ liệu mẫu cho Quán ăn (Tài có thể thêm tiếp ở đây)
            _samplePois.Add(new POI
            {
                PoiId = _nextId++,
                Name = "Ốc Oanh 534",
                Description = "Quán ốc nổi tiếng nhất phố Vĩnh Khánh với món sốt trứng muối.",
                ImageSource = "ocoanh.jpg",
                CategoryId = 1,
                Category = _categories[0],
                Location = new Location { Address = "534 Vĩnh Khánh", Latitude = 10.75883, Longitude = 106.70505 }
            });

            _samplePois.Add(new POI
            {
                PoiId = _nextId++,
                Name = "Phá lấu Dì Nũi",
                Description = "Phá lấu gia truyền cực ngon, ăn kèm bánh mì giòn.",
                ImageSource = "phalau.jpg",
                CategoryId = 3,
                Category = _categories[2],
                Location = new Location { Address = "194 Vĩnh Khánh", Latitude = 10.75940, Longitude = 106.70410 }
            });
        }

        public IActionResult Index()
        {
            return View(_samplePois);
        }

        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_categories, "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        public IActionResult Create(POI poi, double Latitude, double Longitude, string Address)
        {
            // Tự sinh ID và gán dữ liệu liên quan
            poi.PoiId = _nextId++;
            poi.Location = new Location { Latitude = Latitude, Longitude = Longitude, Address = Address };
            poi.Category = _categories.FirstOrDefault(c => c.CategoryId == poi.CategoryId);

            _samplePois.Add(poi);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var poi = _samplePois.FirstOrDefault(p => p.PoiId == id);
            if (poi == null) return NotFound();
            ViewBag.Categories = new SelectList(_categories, "CategoryId", "CategoryName", poi.CategoryId);
            return View(poi);
        }

        [HttpPost]
        public IActionResult Edit(POI poi, double Latitude, double Longitude, string Address)
        {
            var existing = _samplePois.FirstOrDefault(p => p.PoiId == poi.PoiId);
            if (existing != null)
            {
                existing.Name = poi.Name;
                existing.Description = poi.Description;
                existing.CategoryId = poi.CategoryId;
                existing.Category = _categories.FirstOrDefault(c => c.CategoryId == poi.CategoryId);
                existing.Location = new Location { Latitude = Latitude, Longitude = Longitude, Address = Address };
            }
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var poi = _samplePois.FirstOrDefault(p => p.PoiId == id);
            if (poi != null) _samplePois.Remove(poi);
            return RedirectToAction("Index");
        }
    }
}