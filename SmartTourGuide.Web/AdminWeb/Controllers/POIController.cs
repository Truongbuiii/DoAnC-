using AdminWeb.Data;
using AdminWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace AdminWeb.Controllers
{
    public class POIController : Controller
    {
        private readonly AppDbContext _context;

        public POIController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var pois = _context.POIs.ToList();
            return View(pois);
        }

        // mở form
        public IActionResult Create()
        {
            return View();
        }

        // lưu POI
        [HttpPost]
        public IActionResult Create(POI poi)
        {
            _context.POIs.Add(poi);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        public IActionResult Edit(int id)
        {
            var poi = _context.POIs.FirstOrDefault(p => p.PoiId == id);

            if (poi == null)
            {
                return NotFound();
            }

            return View(poi);
        }

        [HttpPost]
        public IActionResult Edit(POI poi)
        {
            var existing = _context.POIs.FirstOrDefault(p => p.PoiId == poi.PoiId);

            if (existing != null)
            {
                existing.Name = poi.Name;
                existing.Latitude = poi.Latitude;
                existing.Longitude = poi.Longitude;
                existing.Description = poi.Description;

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var poi = _context.POIs.FirstOrDefault(p => p.PoiId == id);

            if (poi != null)
            {
                _context.POIs.Remove(poi);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}