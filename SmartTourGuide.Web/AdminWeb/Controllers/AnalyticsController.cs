using AdminWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminWeb.Controllers
{
    public partial class AnalyticsController : Controller
    {
        private readonly AppDbContext _context;

        public AnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Thống kê Top 5 quán ốc được ghé thăm nhiều nhất
            var topPois = await _context.ActivityLogs
                .Include(l => l.Poi)
                .GroupBy(l => l.Poi.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            // 2. Thống kê ngôn ngữ khách sử dụng (VN, EN, KO...)
            var languageStats = await _context.ActivityLogs
                .GroupBy(l => l.LanguageUsed)
                .Select(g => new { Language = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.PoiNames = topPois.Select(x => x.Name).ToArray();
            ViewBag.PoiCounts = topPois.Select(x => x.Count).ToArray();
            ViewBag.LangLabels = languageStats.Select(x => x.Language).ToArray();
            ViewBag.LangCounts = languageStats.Select(x => x.Count).ToArray();

            return View();
        }
    }
}