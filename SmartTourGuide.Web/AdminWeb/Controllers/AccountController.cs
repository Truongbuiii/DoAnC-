using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using AdminWeb.Data;
using AdminWeb.Models;

namespace AdminWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // --- PHẢI CÓ HÀM NÀY ĐỂ HIỆN FORM ĐĂNG NHẬP ---
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            var user = await _context.Admins
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                var claims = new List<Claim> {
            new Claim(ClaimTypes.Name, user.Username!),
            new Claim("FullName", user.FullName ?? ""),
            new Claim(ClaimTypes.Role, user.Role ?? "Owner")
        };

                var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

                // --- THÊM DÒNG NÀY Ở ĐÂY ---
                TempData["SuccessMessage"] = $"Chào mừng {user.Username} đã đăng nhập thành công!";
                // ---------------------------

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "POI");
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng!";
            return View();
        }

        // Sửa Logout thành GET để tránh lỗi 405 khi bấm link trực tiếp
        [HttpGet, HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            // Xóa cookie thủ công cho chắc chắn
            Response.Cookies.Delete("MyCookieAuth");
            return RedirectToAction("Login", "Account");
        }
    }
}