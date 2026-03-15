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

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                }).ToList();

            return View();
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