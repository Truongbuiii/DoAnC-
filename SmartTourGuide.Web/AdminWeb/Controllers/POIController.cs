using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminWeb.Data;
using AdminWeb.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        // 1. TRANG DANH SÁCH + TÌM KIẾM + PHÂN QUYỀN
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            var query = _context.POIs.AsNoTracking().AsQueryable();

            // 2. --- PHÂN QUYỀN DỮ LIỆU TẠI ĐÂY ---
            // Nếu tài khoản KHÔNG PHẢI là admin thì chỉ lọc ra những quán do mình làm chủ
            if (!User.IsInRole("Admin"))
            {
                string currentUserName = User.Identity.Name;
                query = query.Where(p => p.OwnerUsername == currentUserName);
            }
            // Nếu là Admin, sẽ thấy toàn bộ quán và tên chủ sở hữu

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Name.Contains(searchString));
            }

            return View(await query.ToListAsync());
        }

        // 2. TRANG THÊM MỚI (GIAO DIỆN)
        // GET: POI/Create
        public async Task<IActionResult> Create(string? owner)
        {
            // 1. Lấy lại danh sách các chủ quán từ database để đổ vào dropdown
            var owners = await _context.Admins
                .Where(a => a.Role == "Owner")
                .Select(a => a.Username)
                .ToListAsync();

            // 2. Tạo SelectList và chọn sẵn giá trị 'owner' truyền sang
            ViewBag.OwnerList = new SelectList(owners, owner);

            // 3. Khởi tạo model với OwnerUsername mặc định
            var model = new POI { OwnerUsername = owner };

            return View(model);
        }

        // 3. XỬ LÝ LƯU THÊM MỚI (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(POI poi, IFormFile? imageFile)
        {
            // Xóa validation của các field không còn dùng
            ModelState.Remove("DescriptionEn");
            ModelState.Remove("DescriptionZh");
            ModelState.Remove("DescriptionKo");
            ModelState.Remove("DescriptionJa");
            ModelState.Remove("Audios");
            ModelState.Remove("OwnerUsername");


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
                    // THÊM DÒNG NÀY:
                    TempData["SuccessMessage"] = "Chúc mừng! Bạn đã thêm địa điểm '" + poi.Name + "' thành công.";

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi rồi ơi: " + ex.Message);
                }
            }
            return View(poi);
        }

        // 4. TRANG CHỈNH SỬA (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return NotFound();

            if (!User.IsInRole("Admin") && poi.OwnerUsername != User.Identity.Name) return Forbid();

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

            // Xóa validation cho các trường tự động xử lý
            ModelState.Remove("ImageSource");
            ModelState.Remove("Audios");

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
                        // Giữ lại ảnh cũ nếu không đổi
                        _context.Entry(poi).Property(x => x.ImageSource).IsModified = false;
                    }

                    _context.Update(poi);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi sửa: " + ex.Message);
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

            // --- THÊM DÒNG THÔNG BÁO Ở ĐÂY ---
            TempData["SuccessMessage"] = $"Đã xóa địa điểm '{poi.Name}' thành công!";

            return RedirectToAction(nameof(Index));
        }
        // ============================================================
        // API ENDPOINTS (DÙNG CHO MOBILE APP)
        // ============================================================

        // API 1: Lấy toàn bộ POI kèm đầy đủ bản dịch
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
            }).ToListAsync();
            return Json(pois);
        }

        // API 2: Lấy Audio của POI (Đã gỡ bỏ Language, trả về Script và AudioFilePath)
        [AllowAnonymous]
        [HttpGet("/api/v1/audios/{poiId}")]
        public async Task<IActionResult> GetAudioApi(int poiId) // Bỏ tham số 'lang' vì giờ chỉ dùng 1 ngôn ngữ gốc
        {
            // Chỉ cần tìm Audio theo PoiId, không cần check Language nữa
            var audio = await _context.Audios
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.PoiId == poiId);

            if (audio == null) return NotFound();

            // Thay FilePath/Language cũ bằng Script và AudioFilePath mới
            return Json(new
            {
                audio.AudioId,
                audio.AudioName,
                audio.Script,           // Dùng để App đọc TTS
                audio.AudioFilePath,    // Dùng để App phát file mp3 (nếu có)
                audio.PoiId
            });
        }

        [AllowAnonymous]
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
        // Hàm này sẽ nhận ID của quán và Trạng thái mới mà Tài muốn đổi
        public async Task<IActionResult> ChangeStatus(int id, string status)
        {
            // 1. Tìm quán trong SQL Server
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return NotFound();

            // 2. Cập nhật trạng thái mới vào cột Status
            poi.Status = status;

            // 3. Lưu xuống Database
            await _context.SaveChangesAsync();

            // 4. Bắn một thông báo nhỏ cho người dùng biết đã xong
            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái '{poi.Name}' thành {status}!";

            // 5. Quay lại trang danh sách để thấy Badge đổi màu
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        [HttpGet("/api/v1/tours")]
        public async Task<IActionResult> GetToursApi()
        {
            var tours = await _context.Tours.AsNoTracking().ToListAsync();
            var details = await _context.TourDetails.AsNoTracking().ToListAsync();
            var result = tours.Select(t => new {
                t.TourId,
                t.TourName,
                t.Description,
                t.TotalTime,
                t.ImageSource,
                PoiIdsRaw = string.Join(",", details.Where(d => d.TourId == t.TourId).OrderBy(d => d.Order).Select(d => d.PoiId))
            }); 
            return Json(result);
        }
        [AllowAnonymous]
        [HttpGet("/api/v1/audios/all")]
        public async Task<IActionResult> GetAllAudiosApi()
        {
            // Lấy tất cả audio để App đồng bộ về SQLite
            var audios = await _context.Audios.ToListAsync();
            return Json(audios);
        }
    }

    public class ActivityLogDto
    {
        public int PoiId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string LanguageUsed { get; set; } = string.Empty;
        public string DeviceType { get; set; } = "Android";
        public DateTime AccessTime { get; set; }
    }
}