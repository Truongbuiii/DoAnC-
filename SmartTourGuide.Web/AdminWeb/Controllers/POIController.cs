using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminWeb.Data;
using AdminWeb.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering; // Thêm dòng này để dùng SelectList

namespace AdminWeb.Controllers
{
    [Authorize]
    public class POIController : Controller
    {
        private readonly AppDbContext _context;

        public POIController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1. TRANG DANH SÁCH + TÌM KIẾM
        // ============================================================
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            string currentUserName = User.Identity.Name!;
            var query = _context.POIs.AsNoTracking().AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(p => p.OwnerUsername == currentUserName);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Name.Contains(searchString));
            }

            var model = await query.ToListAsync();
            return View(model);
        }

        // ============================================================
        // 2. TRANG THÊM MỚI (GIAO DIỆN) - ĐÃ SỬA BƯỚC 4
        // ============================================================
        public async Task<IActionResult> Create()
        {
            // Lấy danh sách các tài khoản chủ quán để đổ vào Dropdown
            // Chỉ lấy những người không phải là 'admin'
            var owners = await _context.Admins
                .Where(a => a.Username != "admin")
                .Select(a => a.Username)
                .ToListAsync();

            ViewBag.OwnerList = new SelectList(owners);

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
                    // Nếu là Chủ quán tạo, tự động gán OwnerUsername là chính họ
                    // Nếu là Admin tạo, thì nó sẽ lấy giá trị từ Dropdown mà Admin đã chọn
                    if (!User.IsInRole("Admin"))
                    {
                        poi.OwnerUsername = User.Identity.Name;
                    }

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                        if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        string path = Path.Combine(uploadDir, fileName);
                        using (var stream = new FileStream(path, FileMode.Create)) { await imageFile.CopyToAsync(stream); }
                        poi.ImageSource = fileName;
                    }

                    _context.POIs.Add(poi);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                }
            }
            return View(poi);
        }

        // ============================================================
        // 4. TRANG CHỈNH SỬA (GET) - ĐÃ SỬA BƯỚC 4
        // ============================================================
        public async Task<IActionResult> Edit(int id)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return NotFound();

            if (!User.IsInRole("Admin") && poi.OwnerUsername != User.Identity.Name)
            {
                return Forbid();
            }

            // Gửi danh sách chủ quán qua để Admin có thể đổi chủ cho quán này
            var owners = await _context.Admins
                .Where(a => a.Username != "admin")
                .Select(a => a.Username)
                .ToListAsync();

            ViewBag.OwnerList = new SelectList(owners, poi.OwnerUsername);

            return View(poi);
        }

        // 5. XỬ LÝ LƯU DỮ LIỆU SAU KHI SỬA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, POI poi, IFormFile? imageFile)
        {
            if (id != poi.PoiId) return NotFound();

            var existingPoi = await _context.POIs.AsNoTracking().FirstOrDefaultAsync(p => p.PoiId == id);
            if (existingPoi == null) return NotFound();

            if (!User.IsInRole("Admin") && existingPoi.OwnerUsername != User.Identity.Name)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                        using (var stream = new FileStream(path, FileMode.Create)) { await imageFile.CopyToAsync(stream); }
                        poi.ImageSource = fileName;
                    }
                    else
                    {
                        poi.ImageSource = existingPoi.ImageSource;
                    }

                    // Nếu là Admin sửa, thì lấy giá trị OwnerUsername mới từ form (Dropdown)
                    // Nếu là Owner sửa, giữ nguyên OwnerUsername cũ của họ
                    if (!User.IsInRole("Admin"))
                    {
                        poi.OwnerUsername = existingPoi.OwnerUsername;
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
            if (poi == null) return NotFound();

            if (!User.IsInRole("Admin") && poi.OwnerUsername != User.Identity.Name)
            {
                return Forbid();
            }

            _context.POIs.Remove(poi);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}