using AdminWeb.Data;
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
    }
}