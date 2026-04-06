using AdminWeb.Data;
using AdminWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace AdminWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ToursController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ToursController(AppDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Tours
        public async Task<IActionResult> Index()
        {
            var tours = await _context.Tours
                .Include(t => t.TourDetails)
                    .ThenInclude(td => td.Poi)
                .ToListAsync();
            return View(tours);
        }

        // GET: Tours/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tour = await _context.Tours
                .Include(t => t.TourDetails)
                    .ThenInclude(td => td.Poi)
                .FirstOrDefaultAsync(m => m.TourId == id);

            if (tour == null) return NotFound();

            return View(tour);
        }

        // GET: Tours/Create
        public IActionResult Create()
        {
            // Lấy danh sách POI (Quán ăn) để hiển thị checkbox
            ViewBag.Pois = _context.POIs.OrderBy(p => p.Name).ToList();
            return View();
        }

        // POST: Tours/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tour tour, int[] selectedPois, IFormFile? imgFile)
        {
            if (ModelState.IsValid)
            {
                // 1. Xử lý Upload ảnh vào cột ImageSource
                if (imgFile != null)
                {
                    tour.ImageSource = await SaveImage(imgFile);
                }

                _context.Add(tour);
                await _context.SaveChangesAsync();

                // 2. Lưu danh sách điểm dừng
                if (selectedPois != null && selectedPois.Length > 0)
                {
                    for (int i = 0; i < selectedPois.Length; i++)
                    {
                        _context.TourDetails.Add(new TourDetail
                        {
                            TourId = tour.TourId,
                            PoiId = selectedPois[i],
                            Order = i + 1
                        });
                    }
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Pois = _context.POIs.ToList();
            return View(tour);
        }

        // GET: Tours/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tour = await _context.Tours
                .Include(t => t.TourDetails)
                .FirstOrDefaultAsync(m => m.TourId == id);

            if (tour == null) return NotFound();

            ViewBag.AllPois = await _context.POIs.OrderBy(p => p.Name).ToListAsync();
            ViewBag.SelectedPoiIds = tour.TourDetails.Select(td => td.PoiId).ToList();

            return View(tour);
        }

        // POST: Tours/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tour tour, int[] selectedPois, IFormFile? imgFile)
        {
            if (id != tour.TourId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Truy vấn tour cũ để xử lý ảnh (AsNoTracking để tránh lỗi conflict EF)
                    var oldTour = await _context.Tours.AsNoTracking().FirstOrDefaultAsync(t => t.TourId == id);

                    if (imgFile != null)
                    {
                        // Nếu có ảnh mới: Xóa ảnh cũ trên server và lưu ảnh mới
                        DeleteImage(oldTour?.ImageSource);
                        tour.ImageSource = await SaveImage(imgFile);
                    }
                    else
                    {
                        // Nếu không upload ảnh mới: Giữ nguyên tên file cũ trong DB
                        tour.ImageSource = oldTour?.ImageSource;
                    }

                    _context.Update(tour);
                    await _context.SaveChangesAsync();

                    // Cập nhật lại danh sách điểm dừng (Xóa hết cũ - Add mới)
                    var oldDetails = _context.TourDetails.Where(td => td.TourId == id);
                    _context.TourDetails.RemoveRange(oldDetails);

                    if (selectedPois != null)
                    {
                        for (int i = 0; i < selectedPois.Length; i++)
                        {
                            _context.TourDetails.Add(new TourDetail
                            {
                                TourId = id,
                                PoiId = selectedPois[i],
                                Order = i + 1
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TourExists(tour.TourId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.AllPois = _context.POIs.ToList();
            return View(tour);
        }

        // POST: Tours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour != null)
            {
                // Xóa file vật lý trong thư mục trước khi xóa dòng trong DB
                DeleteImage(tour.ImageSource);
                _context.Tours.Remove(tour);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // --- Các hàm hỗ trợ xử lý File ---

        private async Task<string> SaveImage(IFormFile file)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            // Tạo tên file duy nhất bằng Guid để tránh trùng lặp
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
          string path = Path.Combine(wwwRootPath, "images");

            // Tạo thư mục nếu chưa có
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            using (var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return fileName;
        }

        private void DeleteImage(string? fileName)
        {
            // 1. Nếu tên file trống thì không làm gì cả
            if (string.IsNullOrEmpty(fileName)) return;

            // 2. PHẢI kết hợp Thư mục + Tên file thì mới ra đường dẫn file cụ thể
            string path = Path.Combine(_hostEnvironment.WebRootPath, "images", fileName);

            // 3. Kiểm tra xem file đó có tồn tại trên ổ cứng không rồi mới xóa
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
        private bool TourExists(int id)
        {
            return _context.Tours.Any(e => e.TourId == id);
        }
    }
}
