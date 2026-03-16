using Microsoft.EntityFrameworkCore;
using AdminWeb.Models;

namespace AdminWeb.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Bảng cũ đã có
        public DbSet<POI> POIs { get; set; }

        // BỔ SUNG 2 DÒNG NÀY ĐỂ HẾT LỖI
        public DbSet<Category> Categories { get; set; }
        public DbSet<Location> Locations { get; set; }
    }
}