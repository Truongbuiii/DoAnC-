using AdminWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace AdminWeb.Controllers
{
    [Authorize]
    public partial class AnalyticsController : Controller
    {
        private readonly AppDbContext _context;

        public AnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            string currentUserName = User.Identity!.Name!;
            bool isAdmin = User.IsInRole("Admin");

            // 1. Truy vấn gốc kèm theo POI
            var query = _context.ActivityLogs.Include(l => l.Poi).AsQueryable();

            if (isAdmin)
            {
                // --- DÀNH CHO ADMIN ---
                ViewBag.AnalyticsTitle = "Báo cáo Tổng quan Phố Vĩnh Khánh";

                var topPois = await query
                    .GroupBy(l => l.Poi.Name)
                    .Select(g => new { Name = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(5).ToListAsync();

                ViewBag.PoiNames = topPois.Select(x => x.Name).ToArray();
                ViewBag.PoiCounts = topPois.Select(x => x.Count).ToArray();
                ViewBag.ChartLabel = "Số lượt ghé thăm (Toàn phố)";
            }
            else
            {
                // --- DÀNH CHO CHỦ QUÁN ---
                ViewBag.AnalyticsTitle = "Phân tích Hoạt động Cửa hàng";

                var ownerQuery = query.Where(l => l.Poi.OwnerUsername == currentUserName);
                var rawStats = await ownerQuery
                    .GroupBy(l => l.AccessTime.Value.Date) // Thêm .Value trước .Date
                    .Select(g => new { Day = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Day)
                    .ToListAsync();

                var dailyStats = rawStats.Select(x => new {
                    DateLabel = x.Day.ToString("dd/MM"),
                    VisitCount = x.Count
                }).ToList();

                ViewBag.PoiNames = dailyStats.Select(x => x.DateLabel).ToArray();
                ViewBag.PoiCounts = dailyStats.Select(x => x.VisitCount).ToArray();
                ViewBag.ChartLabel = "Lượt khách ghé quán của bạn";
            }

            // 2. THỐNG KÊ NGÔN NGỮ (BẢN TỐI ƯU CHỐNG LỖI DB TRỐNG)
            var currentData = isAdmin ? query : query.Where(l => l.Poi.OwnerUsername == currentUserName);

            var languageData = await currentData
                .GroupBy(l => l.LanguageUsed)
                .Select(g => new { Language = g.Key ?? "Unknown", Count = g.Count() })
                .ToListAsync();

            ViewBag.LangLabels = languageData.Any() ? languageData.Select(x => x.Language).ToArray() : new string[] { "N/A" };
            ViewBag.LangCounts = languageData.Any() ? languageData.Select(x => x.Count).ToArray() : new int[] { 0 };

            // ✅ THÊM: Thiết bị đang active (10 phút gần nhất)
            ViewBag.ActiveDevices = await currentData
                .Where(x => x.AccessTime >= DateTime.Now.AddMinutes(-10))
                .Select(x => x.DeviceType)
                .Distinct()
                .CountAsync();
            ViewBag.IsAdmin = isAdmin;

            return View();
        }
    }
}