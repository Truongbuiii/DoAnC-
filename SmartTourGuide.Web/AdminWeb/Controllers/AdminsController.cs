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
                _context.Admins.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã tạo tài khoản '{model.Username}' thành công. Hãy thêm địa điểm cho chủ quán này!";

                // CHỖ NÀY QUAN TRỌNG: Chuyển hướng kèm theo Username vừa tạo
                return RedirectToAction("Create", "POI", new { owner = model.Username });
            }
            return View(model);
        }

        // Xóa tài khoản (không cho xóa nick admin chính)
        // Xóa tài khoản (không cho xóa nick admin chính)
        public async Task<IActionResult> Delete(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null) return NotFound();

            // Bảo mật: Không cho xóa tài khoản admin hệ thống
            if (admin.Username.ToLower() == "admin")
            {
                TempData["ErrorMessage"] = "Không thể xóa tài khoản Quản trị viên!";
                return RedirectToAction(nameof(Index));
            }

            // 1. Tìm tất cả POI thuộc về user này
            var relatedPois = _context.POIs.Where(p => p.OwnerUsername == admin.Username);

            // 2. Xóa các POI đó trước
            _context.POIs.RemoveRange(relatedPois);

            // 3. Sau đó mới xóa tài khoản Admin
            _context.Admins.Remove(admin);

            await _context.SaveChangesAsync();

            // --- THÊM DÒNG NÀY ĐỂ HIỆN POPUP THÀNH CÔNG ---
            TempData["SuccessMessage"] = $"Đã xóa tài khoản '{admin.Username}' thành công!";

            return RedirectToAction(nameof(Index));
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