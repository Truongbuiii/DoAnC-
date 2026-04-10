using AdminWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminWeb.Controllers
{
    public class ActivityLogsController : Controller
    {
        private readonly AppDbContext _context;

        public ActivityLogsController(AppDbContext context)
        {
            _context = context;
        }

        // Trang danh sách Log
        public async Task<IActionResult> Index()
        {
            // Lấy dữ liệu kèm thông tin POI, sắp xếp cái mới nhất lên đầu
            var logs = await _context.ActivityLogs
                .Include(a => a.Poi)
                .OrderByDescending(a => a.AccessTime)
                .ToListAsync();

            return View(logs);
        }
        // --- THÊM HÀM NÀY ĐỂ NHẬN DỮ LIỆU TỪ APP ---
        [AllowAnonymous] // Cho phép App gửi log mà không cần đăng nhập giao diện Web
        [HttpPost("/api/v1/analytics/sync")] // Khớp với địa chỉ App gọi
        public async Task<IActionResult> SyncLogsFromApp([FromBody] List<AdminWeb.Models.ActivityLog> logs)
        {
            if (logs == null || !logs.Any())
            {
                return BadRequest("Không có dữ liệu gửi lên.");
            }

            try
            {
                // Ghi danh sách log mới vào Database
                _context.ActivityLogs.AddRange(logs);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = $"Đã nhận {logs.Count} logs." });
            }
            catch (Exception ex)
            {
                // Nếu lỗi (vượt quá dung lượng hoặc sai định dạng), báo về cho App
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
    }
}