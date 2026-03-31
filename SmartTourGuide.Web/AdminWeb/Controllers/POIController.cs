using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminWeb.Data;
using AdminWeb.Models;

namespace AdminWeb.Controllers
{
    public class POIController : Controller
    {
        private readonly AppDbContext _context;

        public POIController(AppDbContext context)
        {
            _context = context;
        }

        // 1. TRANG DANH SÁCH + TÌM KIẾM
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            // Giờ không cần .Include nữa vì dữ liệu đã nằm chung 1 bảng POIs
            var query = _context.POIs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Name.Contains(searchString));
            }

            var model = await query.ToListAsync();
            return View(model);
        }

        // 2. TRANG THÊM MỚI (GIAO DIỆN)
        public IActionResult Create()
        {
            return View();
        }

        // 3. XỬ LÝ LƯU DỮ LIỆU THÊM MỚI (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(POI poi, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý lưu File ảnh vào wwwroot/images
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                        if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        string path = Path.Combine(uploadDir, fileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        poi.ImageSource = fileName;
                    }

                    _context.POIs.Add(poi);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi rồi Tài ơi: " + ex.Message);
                }
            }
            return View(poi);
        }

        // 4. TRANG CHỈNH SỬA (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return NotFound();
            return View(poi);
        }

        // 5. XỬ LÝ LƯU DỮ LIỆU SAU KHI SỬA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, POI poi, IFormFile? imageFile)
        {
            if (id != poi.PoiId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Nếu có ảnh mới thì cập nhật, không thì giữ ảnh cũ
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                        using (var stream = new FileStream(path, FileMode.Create)) { await imageFile.CopyToAsync(stream); }
                        poi.ImageSource = fileName;
                    }
                    else
                    {
                        // Giữ lại tên ảnh cũ nếu không upload ảnh mới
                        _context.Entry(poi).Property(x => x.ImageSource).IsModified = false;
                    }

                    _context.Update(poi);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.POIs.Any(e => e.PoiId == poi.PoiId)) return NotFound();
                    else throw;
                }
            }
            return View(poi);
        }

        // 6. XỬ LÝ XÓA
        public async Task<IActionResult> Delete(int id)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi != null)
            {
                _context.POIs.Remove(poi);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}