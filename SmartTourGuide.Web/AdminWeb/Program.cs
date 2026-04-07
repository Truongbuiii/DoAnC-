using AdminWeb.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CẤU HÌNH DỊCH VỤ (SERVICES) ---

builder.Services.AddControllersWithViews();

// Kết nối Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Cấu hình xác thực Cookie ---
builder.Services.AddAuthentication("MyCookieAuth") // Tên Schema phải khớp với SignInAsync
    .AddCookie("MyCookieAuth", options =>
    {
        options.Cookie.Name = "MyCookieAuth";
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout"; // Thêm dòng này cho chắc
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(24); // Cho hẳn 1 ngày cho thoải mái test
        options.SlidingExpiration = true;

        // QUAN TRỌNG: Đảm bảo Cookie hoạt động tốt trên Localhost
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });
var app = builder.Build();

// --- 2. CẤU HÌNH PIPELINE (MIDDLEWARE) ---
// Thứ tự ở đây CỰC KỲ QUAN TRỌNG, không được đảo lộn

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Để hiện CSS, JS, Hình ảnh

app.UseRouting();

// BẮT BUỘC: Authentication phải đứng TRƯỚC Authorization
app.UseAuthentication();
app.UseAuthorization();

// 3. ĐỊNH NGHĨA ĐƯỜNG DẪN (ROUTES)
app.MapControllerRoute(
    name: "default",
    // Khi mới mở web, nó sẽ chạy vào Account/Login trước để bắt đăng nhập
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();