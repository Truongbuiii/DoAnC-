using AdminWeb.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Thêm dịch vụ MVC
builder.Services.AddControllersWithViews();

// 2. Kết nối Database từ file appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 3. Cấu hình Pipeline xử lý yêu cầu
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Thay cho MapStaticAssets nếu bản .NET cũ hơn

app.UseRouting();
app.UseAuthorization();

// 4. Định nghĩa đường dẫn mặc định
app.MapControllerRoute(
    name: "default",
    // Sửa Home thành POI để khi chạy nó hiện thẳng danh sách quán ốc của Tài
    pattern: "{controller=POI}/{action=Index}/{id?}");

app.Run();