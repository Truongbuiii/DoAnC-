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
            var query = _context.ActivityLogs.Include(l => l.Poi).AsQueryable();

            // 1. Tính số người đang Online (Chỉ Admin mới cần biết tổng)
            if (isAdmin)
            {
                ViewBag.ActiveUsers = await _context.ActivityLogs
                    .Where(l => l.AccessTime >= DateTime.Now.AddMinutes(-5))
                    .Select(l => l.DeviceId).Distinct().CountAsync();
            }

            if (isAdmin)
            {
                ViewBag.AnalyticsTitle = "Báo cáo Tổng quan Phố Vĩnh Khánh";
                ViewBag.ChartType = "bar"; // Admin dùng biểu đồ cột để so sánh

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
                ViewBag.AnalyticsTitle = "Phân tích Hoạt động Cửa hàng";
                ViewBag.ChartType = "line"; // Chủ quán dùng biểu đồ đường cho xu hướng

                var ownerQuery = query.Where(l => l.Poi.OwnerUsername == currentUserName);

                // Lấy dữ liệu 7 ngày gần nhất (kể cả hôm nay)
                DateTime startDate = DateTime.Today.AddDays(-6);
                var rawStats = await ownerQuery
                    .Where(l => l.AccessTime >= startDate)
                    .GroupBy(l => l.AccessTime.Value.Date)
                    .Select(g => new { Day = g.Key, Count = g.Count() })
                    .ToListAsync();

                // LOGIC QUAN TRỌNG: Lấp đầy các ngày bị trống dữ liệu bằng 0
                var labels = new List<string>();
                var counts = new List<int>();

                for (int i = 0; i < 7; i++)
                {
                    DateTime date = startDate.AddDays(i);
                    labels.Add(date.ToString("dd/MM"));
                    var stat = rawStats.FirstOrDefault(x => x.Day == date);
                    counts.Add(stat?.Count ?? 0); // Nếu không có khách thì gán bằng 0
                }

                ViewBag.PoiNames = labels.ToArray();
                ViewBag.PoiCounts = counts.ToArray();
                ViewBag.ChartLabel = "Lượt khách ghé quán (7 ngày qua)";
            }

            // 2. THỐNG KÊ NGÔN NGỮ (BẢN TỐI ƯU CHỐNG LỖI DB TRỐNG)
            var currentData = isAdmin ? query : query.Where(l => l.Poi.OwnerUsername == currentUserName);

            var rawLanguageData = await currentData
                .GroupBy(l => l.LanguageUsed)
                .Select(g => new { Language = g.Key ?? "Unknown", Count = g.Count() })
                .ToListAsync();

            var languageData = rawLanguageData
                .GroupBy(x => NormalizeLanguage(x.Language))
                .Select(g => new { Language = g.Key, Count = g.Sum(x => x.Count) })
                .OrderByDescending(x => x.Count)
                .ToList();

            ViewBag.LangLabels = languageData.Any() ? languageData.Select(x => x.Language).ToArray() : new string[] { "N/A" };
            ViewBag.LangCounts = languageData.Any() ? languageData.Select(x => x.Count).ToArray() : new int[] { 0 };

           
            // ✅ Ô 1: Đang online thật sự (heartbeat trong 1 phút)
            ViewBag.OnlineDevices = await _context.DeviceSessions
                .Where(s => s.IsActive && s.LastSeen >= DateTime.Now.AddMinutes(-1))
                .CountAsync();

            // ✅ Ô 2: Lịch sử hoạt động 10 phút (đổi từ DeviceType sang DeviceId)
            ViewBag.ActiveDevices = await currentData
                .Where(x => x.AccessTime >= DateTime.Now.AddMinutes(-10))
                .Select(x => x.DeviceId)
                .Distinct()
                .CountAsync();

            ViewBag.TotalVisits = await currentData.CountAsync();
            ViewBag.IsAdmin = isAdmin;

            return View();
        }

        private static string NormalizeLanguage(string lang)
        {
            if (string.IsNullOrEmpty(lang)) return "Unknown";
            return lang.ToUpper() switch
            {
                "EN" or "EN-US" => "English",
                "VI" or "VI-VN" => "Tiếng Việt",
                "ZH" or "ZH-CN" => "中文",
                "KO" or "KO-KR" => "한국어",
                "JA" or "JA-JP" => "日本語",
                _ => lang
            };
        }
    }
}