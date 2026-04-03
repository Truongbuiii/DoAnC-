using AdminWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace AdminWeb.Controllers
{
    [Authorize] // Bắt buộc đăng nhập mới xem được thống kê
    public partial class AnalyticsController : Controller
    {
        private readonly AppDbContext _context;

        public AnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy thông tin người dùng hiện tại
            string currentUserName = User.Identity!.Name!;
            bool isAdmin = User.IsInRole("Admin");

            // Khởi tạo truy vấn gốc từ bảng ActivityLogs
            var query = _context.ActivityLogs.Include(l => l.Poi).AsQueryable();

            // ============================================================
            // LOGIC PHÂN QUYỀN DỮ LIỆU
            // ============================================================
            if (!isAdmin)
            {
                // Nếu là Chủ quán: Chỉ lọc những bản ghi thuộc về quán của họ
                query = query.Where(l => l.Poi.OwnerUsername == currentUserName);
                ViewBag.AnalyticsTitle = "Thống kê hoạt động của quán: " + currentUserName;
            }
            else
            {
                ViewBag.AnalyticsTitle = "Báo cáo tổng quan toàn phố Vĩnh Khánh";
            }

            // 1. Thống kê Top quán được ghé thăm
            // (Nếu là Chủ quán thì nó sẽ chỉ hiện 1 cột của chính họ, hoặc các chi nhánh của họ nếu có)
            var topPois = await query
                .GroupBy(l => l.Poi.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            // 2. Thống kê ngôn ngữ khách sử dụng (Phân tích tệp khách hàng)
            var languageStats = await query
                .GroupBy(l => l.LanguageUsed)
                .Select(g => new { Language = g.Key, Count = g.Count() })
                .ToListAsync();

            // Đổ dữ liệu ra ViewBag để vẽ biểu đồ (Chart.js)
            ViewBag.PoiNames = topPois.Select(x => x.Name).ToArray();
            ViewBag.PoiCounts = topPois.Select(x => x.Count).ToArray();
            ViewBag.LangLabels = languageStats.Select(x => x.Language).ToArray();
            ViewBag.LangCounts = languageStats.Select(x => x.Count).ToArray();
            ViewBag.IsAdmin = isAdmin;

            return View();
        }
    }
}