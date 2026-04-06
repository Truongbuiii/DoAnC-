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

            // 1. Khởi tạo truy vấn gốc
            var query = _context.POIs.AsNoTracking().AsQueryable();

            // 2. --- PHÂN QUYỀN DỮ LIỆU TẠI ĐÂY ---
            // Nếu tài khoản KHÔNG PHẢI là admin thì chỉ lọc ra những quán do mình làm chủ
            if (!User.IsInRole("Admin"))
            {
                string currentUserName = User.Identity.Name;
                query = query.Where(p => p.OwnerUsername == currentUserName);
            }

            // 3. Xử lý tìm kiếm (nếu có)
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Name.Contains(searchString));
            }

            var model = await query.ToListAsync();
            return View(model);
        }
        // 2. TRANG THÊM MỚI (GIAO DIỆN)
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

        // 3. XỬ LÝ LƯU THÊM MỚI
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

        // 5. XỬ LÝ LƯU SAU KHI SỬA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, POI poi, IFormFile? imageFile)
        {
            if (id != poi.PoiId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                        using (var stream = new FileStream(path, FileMode.Create))
                            await imageFile.CopyToAsync(stream);
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
            if (poi == null) return NotFound();

            if (!User.IsInRole("Admin") && poi.OwnerUsername != User.Identity.Name)
            {
                return Forbid();
            }

            _context.POIs.Remove(poi);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // API ENDPOINTS CHO MOBILE APP
        // ============================================================

        // API 1: GET /api/v1/pois
        [AllowAnonymous]
        [HttpGet("/api/v1/pois")]
        public async Task<IActionResult> GetPoisApi()
        {
            var pois = await _context.POIs.AsNoTracking().Select(p => new
            {
                p.PoiId,
                p.Name,
                p.Category,
                p.Latitude,
                p.Longitude,
                p.TriggerRadius,
                p.ImageSource,
                p.DescriptionVi,
                p.DescriptionEn,
                p.DescriptionZh,
                p.DescriptionKo,
                p.DescriptionJa
            }).ToListAsync();
            return Json(pois);
        }

        // API 2: GET /api/v1/audios/{poiId}?lang={langCode}
        [AllowAnonymous]
        [HttpGet("/api/v1/audios/{poiId}")]
        public async Task<IActionResult> GetAudioApi(int poiId, string lang = "VN")
        {
            var audio = await _context.Audios
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.PoiId == poiId && a.Language == lang);
            if (audio == null) return NotFound();
            return Json(new
            {
                audio.AudioId,
                audio.AudioName,
                audio.FilePath,
                audio.Language,
                audio.PoiId
            });
        }

        // API 3: POST /api/v1/analytics/sync
        [AllowAnonymous]
        [HttpPost("/api/v1/analytics/sync")]
        public async Task<IActionResult> SyncAnalyticsApi([FromBody] List<ActivityLogDto> logs)
        {
            if (logs == null || !logs.Any()) return BadRequest();
            foreach (var log in logs)
            {
                _context.ActivityLogs.Add(new ActivityLog
                {
                    PoiId = log.PoiId,
                    ActionType = log.ActionType,
                    LanguageUsed = log.LanguageUsed,
                    DeviceType = log.DeviceType,
                    AccessTime = log.AccessTime
                });
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true, synced = logs.Count });
        }
        // API 4: GET /api/v1/tours
        [AllowAnonymous]
        [HttpGet("/api/v1/tours")]
        public async Task<IActionResult> GetToursApi()
        {
            var tours = await _context.Tours.AsNoTracking().ToListAsync();
            var details = await _context.TourDetails.AsNoTracking().ToListAsync();

            var result = tours.Select(t => new
            {
                TourId = t.TourId,
                TourName = t.TourName,
                Description = t.Description,
                TotalTime = t.TotalTime,
                ImageSource = t.ImageSource,
                PoiIdsRaw = string.Join(",", details
                    .Where(d => d.TourId == t.TourId)
                    .OrderBy(d => d.Order)
                    .Select(d => d.PoiId))
            });

            return Json(result);
        }
    }

    // DTO cho Analytics Sync
    public class ActivityLogDto
    {
        public int PoiId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string LanguageUsed { get; set; } = string.Empty;
        public string DeviceType { get; set; } = "Android";
        public DateTime AccessTime { get; set; }
    }



}