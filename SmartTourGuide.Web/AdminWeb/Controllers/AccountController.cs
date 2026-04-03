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

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Truyền đường dẫn cũ sang View để sau khi đăng nhập còn biết đường mà quay lại
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            // 1. Kiểm tra tài khoản trong DB
            var user = await _context.Admins
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                // 2. Tạo danh sách quyền hạn (Claims)
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, user.Username!),
                    new Claim("FullName", user.FullName ?? "")
                };

                // Phân vai trò: Admin hoặc Owner
                if (user.Username!.ToLower() == "admin")
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Owner"));
                }

                var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                // 3. Đăng nhập
                await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

                // --- PHẦN SỬA MỚI: ĐIỀU HƯỚNG THÔNG MINH ---
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "POI");
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng!";
            return View();
        }

        [HttpPost] // Thêm cái này để nhận lệnh từ form ẩn phía trên
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            HttpContext.Response.Cookies.Delete("MyCookieAuth");
            return RedirectToAction("Login", "Account");
        }
    }
}