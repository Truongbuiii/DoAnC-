using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminWeb.Data;
using AdminWeb.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AdminWeb.Controllers
{
    public class POIController : Controller
    {
        private readonly AppDbContext _context;

        public POIController(AppDbContext context)
        {
            _context = context;
        }

        // 1. TRANG DANH SÁCH + TÌM KIẾM (Đã cập nhật để khớp với Index.cshtml)
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var pois = _context.POIs
                .Include(p => p.Category)
                .Include(p => p.Location)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                // Tìm kiếm theo tên quán
                pois = pois.Where(s => s.Name.Contains(searchString));
            }

            return View(await pois.ToListAsync());
        }

        // 2. TRANG THÊM MỚI (GIAO DIỆN)
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        // 3. XỬ LÝ LƯU DỮ LIỆU THÊM MỚI (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(POI poi, IFormFile? imageFile, double Latitude, double Longitude, double TriggerRadius, string Address)
        {
            if (string.IsNullOrEmpty(poi.Name))
            {
                ModelState.AddModelError("Name", "Tài ơi, bạn bắt buộc phải nhập Tên địa điểm nhé!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // A. Tạo Location mới
                    var newLoc = new Location { Latitude = Latitude, Longitude = Longitude, TriggerRadius = TriggerRadius, Address = Address };
                    _context.Locations.Add(newLoc);
                    await _context.SaveChangesAsync();

                    // B. Xử lý ảnh
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                        using (var stream = new FileStream(path, FileMode.Create)) { await imageFile.CopyToAsync(stream); }
                        poi.ImageSource = fileName;
                    }

                    // C. Gán ID và lưu POI
                    poi.LocationId = newLoc.LocationId;
                    _context.POIs.Add(poi);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex) { ModelState.AddModelError("", "Lỗi: " + ex.Message); }
            }
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "CategoryName", poi.CategoryId);
            return View(poi);
        }

        // 4. TRANG CHỈNH SỬA (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var poi = await _context.POIs.Include(p => p.Location).FirstOrDefaultAsync(m => m.PoiId == id);
            if (poi == null) return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "CategoryName", poi.CategoryId);
            return View(poi);
        }

        // 5. XỬ LÝ LƯU DỮ LIỆU SAU KHI SỬA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, POI poi, IFormFile? imageFile, double Latitude, double Longitude, double TriggerRadius, string Address)
        {
            if (id != poi.PoiId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật tọa độ
                    var loc = await _context.Locations.FindAsync(poi.LocationId);
                    if (loc != null)
                    {
                        loc.Latitude = Latitude; loc.Longitude = Longitude;
                        loc.TriggerRadius = TriggerRadius; loc.Address = Address;
                        _context.Update(loc);
                    }

                    // Cập nhật ảnh mới nếu có
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                        using (var stream = new FileStream(path, FileMode.Create)) { await imageFile.CopyToAsync(stream); }
                        poi.ImageSource = fileName;
                    }

                    _context.Update(poi);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) { return NotFound(); }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "CategoryName", poi.CategoryId);
            return View(poi);
        }

        // 6. XỬ LÝ XÓA
        public async Task<IActionResult> Delete(int id)
        {
            var poi = await _context.POIs.Include(p => p.Location).FirstOrDefaultAsync(m => m.PoiId == id);
            if (poi != null)
            {
                if (poi.Location != null) _context.Locations.Remove(poi.Location);
                _context.POIs.Remove(poi);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}