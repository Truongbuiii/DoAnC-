using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdminWeb.Data;
using AdminWeb.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace AdminWeb.Controllers
{
    public class AudiosController : Controller
    {
        private readonly AppDbContext _context;

        public AudiosController(AppDbContext context)
        {
            _context = context;
        }

        // 1. TRANG DANH SÁCH: Hiển thị kèm tên Quán ốc (POI)
        public async Task<IActionResult> Index()
        {
            var audios = await _context.Audios.Include(a => a.Poi).ToListAsync();
            return View(audios);
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

        // 3. TẠO MỚI (GET): Load danh sách POI vào Dropdown
        public IActionResult Create()
        {
            ViewBag.PoiId = new SelectList(_context.POIs, "PoiId", "Name");
            return View();
        }

        // 4. TẠO MỚI (POST): Xử lý lưu File và lưu Data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Audio audio, IFormFile audioFile)
        {
            if (audioFile != null && audioFile.Length > 0)
            {
                // Tự động tạo thư mục uploads nếu chưa có
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                // Lưu file vật lý
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(audioFile.FileName);
                var filePath = Path.Combine(uploadDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await audioFile.CopyToAsync(stream);
                }
                audio.FilePath = "/uploads/" + fileName;
            }

            ModelState.Remove("Poi"); // Bỏ qua kiểm tra ràng buộc object Poi
            if (ModelState.IsValid)
            {
                _context.Add(audio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.PoiId = new SelectList(_context.POIs, "PoiId", "Name", audio.PoiId);
            return View(audio);
        }

        // 5. CHỈNH SỬA (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var audio = await _context.Audios.FindAsync(id);
            if (audio == null) return NotFound();

            ViewBag.PoiId = new SelectList(_context.POIs, "PoiId", "Name", audio.PoiId);
            return View(audio);
        }

        // 6. CHỈNH SỬA (POST): Có xử lý nếu Tài muốn đổi file nhạc mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Audio audio, IFormFile? audioFile)
        {
            if (id != audio.AudioId) return NotFound();

            if (audioFile != null && audioFile.Length > 0)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(audioFile.FileName);
                var filePath = Path.Combine(uploadDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await audioFile.CopyToAsync(stream);
                }
                audio.FilePath = "/uploads/" + fileName;
            }

            ModelState.Remove("Poi");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(audio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AudioExists(audio.AudioId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.PoiId = new SelectList(_context.POIs, "PoiId", "Name", audio.PoiId);
            return View(audio);
        }

        // 7. XÓA (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var audio = await _context.Audios
                .Include(a => a.Poi)
                .FirstOrDefaultAsync(m => m.AudioId == id);

            if (audio == null) return NotFound();

            return View(audio);
        }

        // 8. XÓA (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var audio = await _context.Audios.FindAsync(id);
            if (audio != null)
            {
                // Xóa file vật lý trong thư mục uploads để đỡ nặng máy (nếu cần)
                if (!string.IsNullOrEmpty(audio.FilePath))
                {
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", audio.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
                }

                _context.Audios.Remove(audio);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AudioExists(int id)
        {
            return _context.Audios.Any(e => e.AudioId == id);
        }
    }
}