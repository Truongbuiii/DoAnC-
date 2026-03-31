using AdminWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminWeb.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<POI> POIs { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Audio> Audios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình bảng POIs
            modelBuilder.Entity<POI>(entity =>
            {
                entity.ToTable("POIs");
                entity.HasKey(e => e.PoiId);
                entity.Property(e => e.PoiId).HasColumnName("PoiId");
            });

            // Cấu hình bảng Audios
            modelBuilder.Entity<Audio>(entity =>
            {
                entity.ToTable("Audios");
                entity.HasKey(e => e.AudioId);

                entity.HasOne(a => a.Poi)
                      .WithMany(p => p.Audios)
                      .HasForeignKey(a => a.PoiId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

        