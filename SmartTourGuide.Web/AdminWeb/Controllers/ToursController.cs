using AdminWeb.Data;
using AdminWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ToursController : Controller
    {
        private readonly AppDbContext _context;

        public ToursController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Tours
        public async Task<IActionResult> Index()
        {
            // Phải dùng Include thì nó mới lôi được tên Quán ốc từ bảng POI ra cho Tài xem
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
                .ThenInclude(td => td.Poi) // Cần dòng này để hiện tên quán ốc
                .FirstOrDefaultAsync(m => m.TourId == id);

            if (tour == null) return NotFound();

            return View(tour);
        }

        // GET: Tours/Create
        // GET: Tours/Create
        public IActionResult Create()
        {
            // Lấy danh sách 10 quán ốc của Tài để hiện lên checkbox
            ViewBag.PoiList = _context.POIs.OrderBy(p => p.Name).ToList();
            return View();
        }

        // POST: Tours/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tour tour, int[] selectedPois)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tour);
                await _context.SaveChangesAsync(); // Lưu Tour trước để lấy ID

                // Sau đó lưu các điểm dừng vào TourDetails
                if (selectedPois != null)
                {
                    int order = 1;
                    foreach (var poiId in selectedPois)
                    {
                        var detail = new TourDetail
                        {
                            TourId = tour.TourId,
                            PoiId = poiId,
                            Order = order++
                        };
                        _context.TourDetails.Add(detail);
                    }
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.PoiList = _context.POIs.ToList();
            return View(tour);
        }
        // GET: Tours/Edit/5
        // GET: Tours/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tour = await _context.Tours
                .Include(t => t.TourDetails)
                .FirstOrDefaultAsync(m => m.TourId == id);

            if (tour == null) return NotFound();

            // Gửi danh sách tất cả quán ốc qua để chọn thêm
            ViewBag.AllPois = await _context.POIs.OrderBy(p => p.Name).ToListAsync();

            // Lấy danh sách ID các quán đã có trong Tour này
            ViewBag.SelectedPoiIds = tour.TourDetails.Select(td => td.PoiId).ToList();

            return View(tour);
        }

        // POST: Tours/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tour tour, int[] selectedPois)
        {
            if (id != tour.TourId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tour);
                    await _context.SaveChangesAsync();

                    // 1. Xóa hết các điểm dừng cũ của Tour này
                    var oldDetails = _context.TourDetails.Where(td => td.TourId == id);
                    _context.TourDetails.RemoveRange(oldDetails);
                    await _context.SaveChangesAsync();

                    // 2. Thêm lại danh sách mới từ checkbox
                    if (selectedPois != null)
                    {
                        int order = 1;
                        foreach (var poiId in selectedPois)
                        {
                            _context.TourDetails.Add(new TourDetail
                            {
                                TourId = id,
                                PoiId = poiId,
                                Order = order++
                            });
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Tours.Any(e => e.TourId == tour.TourId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tour);
        }
        // GET: Tours/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours
                .FirstOrDefaultAsync(m => m.TourId == id);
            if (tour == null)
            {
                return NotFound();
            }

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
                _context.Tours.Remove(tour);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TourExists(int id)
        {
            return _context.Tours.Any(e => e.TourId == id);
        }
    }
}
