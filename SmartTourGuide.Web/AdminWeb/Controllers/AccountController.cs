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
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
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

                // Phân vai trò: Nếu là 'admin' thì gán Role Admin, ngược lại là Owner
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

                // 3. Đăng nhập với Scheme "MyCookieAuth" (KHÔNG ĐƯỢC SAI TÊN NÀY)
                await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

                return RedirectToAction("Index", "POI");
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng!";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            // 4. Đăng xuất cũng phải dùng "MyCookieAuth"
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Login");
        }
    }
}