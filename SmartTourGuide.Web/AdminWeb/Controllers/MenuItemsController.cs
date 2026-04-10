using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminWeb.Data;
using AdminWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace AdminWeb.Controllers
{
    [ApiController]
    public class MenuItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MenuItemsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /api/v1/menuitems
        [HttpGet("/api/v1/menuitems")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllMenuItems()
        {
            var items = await _context.MenuItems
                .AsNoTracking()
                .Select(m => new
                {
                    m.MenuId,
                    m.PoiId,
                    m.DishName,
                    m.Price,
                    m.ImageSource,
                    m.IsRecommended
                })
                .ToListAsync();

            return Ok(items);
        }
    }
}
