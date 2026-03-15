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

        public DbSet<POI> POIs { get; set; }
    }
}