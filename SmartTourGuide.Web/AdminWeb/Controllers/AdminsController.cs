using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminWeb.Data;
using AdminWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace AdminWeb.Controllers
{
    [Authorize(Roles = "Admin")] // Chỉ Admin mới vào được đây
    public class AdminsController : Controller
    {
        private readonly AppDbContext _context;

        public AdminsController(AppDbContext context)
        {
            _context = context;
        }

        // Hiện danh sách tài khoản
        public async Task<IActionResult> Index()
        {
            var users = await _context.Admins.ToListAsync();
            return View(users);
        }

        // Giao diện tạo mới
        public IActionResult Create() => View();

        // Xử lý lưu tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Admin model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Admins.AnyAsync(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
                    return View(model);
                }
                _context.Admins.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // Xóa tài khoản (không cho xóa nick admin chính)
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Admins.FindAsync(id);
            if (user != null && user.Username != "admin")
            {
                _context.Admins.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        // GET: Admins/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null) return NotFound();
            return View(admin);
        }

        // POST: Admins/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Admin admin)
        {
            if (id != admin.AdminId) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(admin);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(admin);
        }
    }
}