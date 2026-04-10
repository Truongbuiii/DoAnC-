using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdminWeb.Data;
using AdminWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace AdminWeb.Controllers
{
    [Authorize] // Bảo mật: Phải đăng nhập mới được vào
    public class AudiosController : Controller
    {
        private readonly AppDbContext _context;

        public AudiosController(AppDbContext context)
        {
            _context = context;
        }

        // 1. TRANG DANH SÁCH
        public async Task<IActionResult> Index()
        {
            string currentUserName = User.Identity?.Name;
            var query = _context.Audios.Include(a => a.Poi).AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(a => a.Poi != null && a.Poi.OwnerUsername == currentUserName);
            }

            return View(await query.ToListAsync());
        }

        // 2. CHI TIẾT
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var audio = await _context.Audios
                .Include(a => a.Poi)
                .FirstOrDefaultAsync(m => m.AudioId == id);

            if (audio == null) return NotFound();
            return View(audio);
        }

        // 3. TẠO MỚI (GET): Luôn cho vào trang Create nhưng lọc danh sách
        [HttpGet]
        public IActionResult Create()
        {
            var currentUser = User.Identity?.Name;
            var poiIdsWithAudio = _context.Audios.Select(a => a.PoiId).ToList();

            List<POI> availablePOIs;

            if (User.IsInRole("Admin"))
            {
                availablePOIs = _context.POIs
                    .Where(p => !poiIdsWithAudio.Contains(p.PoiId))
                    .ToList();
            }
            else
            {
                availablePOIs = _context.POIs
                    .Where(p => p.OwnerUsername == currentUser && !poiIdsWithAudio.Contains(p.PoiId))
                    .ToList();
            }

            // Luôn tạo SelectList, dù danh sách có rỗng hay không
            ViewData["PoiId"] = new SelectList(availablePOIs, "PoiId", "Name");

            return View();
        }

        // 4. TẠO MỚI (POST): Đã thêm Upload File, Đổi FilePath thành Script VÀ CHẶN TRÙNG LẶP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AudioId,AudioName,Description,Script,PoiId")] Audio audio, IFormFile? audioFile)
        {
            // --- THÊM ĐOẠN KIỂM TRA TRÙNG LẶP NÀY VÀO ---
            // Kiểm tra xem POI này đã có Kịch bản nào chưa
            var existingAudio = await _context.Audios.FirstOrDefaultAsync(a => a.PoiId == audio.PoiId);
            if (existingAudio != null)
            {
                // Nếu đã có, báo lỗi và trả lại View
                ModelState.AddModelError("PoiId", "Địa điểm này ĐÃ CÓ Kịch bản. Vui lòng vào mục 'Chỉnh sửa' thay vì tạo mới.");
                ViewData["PoiId"] = new SelectList(_context.POIs, "PoiId", "Name", audio.PoiId);
                return View(audio);
            }
            if (audioFile != null && audioFile.Length > 0)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(audioFile.FileName);
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await audioFile.CopyToAsync(stream);
                }
                audio.AudioFilePath = "/uploads/" + fileName; // Lưu đường dẫn file
            }

            ModelState.Remove("Poi");
            if (ModelState.IsValid)
            {
                _context.Add(audio);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm Kịch bản/Audio thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["PoiId"] = new SelectList(_context.POIs, "PoiId", "Name", audio.PoiId);
            return View(audio);
        }

        // 5. CHỈNH SỬA (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var audio = await _context.Audios.FindAsync(id);
            if (audio == null) return NotFound();

            ViewData["PoiId"] = new SelectList(_context.POIs, "PoiId", "Name", audio.PoiId);
            return View(audio);
        }

        // 6. CHỈNH SỬA (POST): Có xử lý Upload file mới VÀ Xóa file cũ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AudioId,AudioName,Description,Script,AudioFilePath,PoiId")] Audio audio, IFormFile? audioFile, bool removeAudio = false)
        {
            if (id != audio.AudioId) return NotFound();

            // 1. Nếu Admin chọn XÓA FILE CŨ
            if (removeAudio && !string.IsNullOrEmpty(audio.AudioFilePath))
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", audio.AudioFilePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath); // Xóa file vật lý cho nhẹ server
                }
                audio.AudioFilePath = null; // Gắn Null vào Database để App hiểu là phải dùng TTS
            }

            // 2. Nếu Admin TẢI LÊN FILE MỚI (Sẽ ghi đè file cũ nếu Admin quên tick xóa)
            if (audioFile != null && audioFile.Length > 0)
            {
                // Nếu đang có file cũ, xóa file cũ đi trước khi up file mới
                if (!string.IsNullOrEmpty(audio.AudioFilePath))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", audio.AudioFilePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(audioFile.FileName);
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await audioFile.CopyToAsync(stream);
                }
                audio.AudioFilePath = "/uploads/" + fileName; // Lưu đường dẫn file mới
            }

            ModelState.Remove("Poi");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(audio);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AudioExists(audio.AudioId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PoiId"] = new SelectList(_context.POIs, "PoiId", "Name", audio.PoiId);
            return View(audio);
        }

        // 7. XÓA (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var audio = await _context.Audios
                .Include(a => a.Poi)
                .FirstOrDefaultAsync(a => a.AudioId == id.Value);

            if (audio == null) return NotFound();

            return View(audio);
        }

        // 8. XÓA (POST): Dọn dẹp cả file vật lý để nhẹ server
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var audio = await _context.Audios.FindAsync(id);
            if (audio != null)
            {
                // Xóa file vật lý trong thư mục uploads
                if (!string.IsNullOrEmpty(audio.AudioFilePath))
                {
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", audio.AudioFilePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
                }

                _context.Audios.Remove(audio);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AudioExists(int id)
        {
            return _context.Audios.Any(e => e.AudioId == id);
        }
    }
}