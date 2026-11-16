using CivicAlert.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CivicAlert.Data;

public class CivicAlertDbContext(DbContextOptions<CivicAlertDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<ReportModel> Reports => Set<ReportModel>();
    public DbSet<DemoModel> Demos => Set<DemoModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AppUser table configuration
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired();
            entity.Property(u => u.Password).IsRequired();

            // Store HashSet<Role> as JSON string in PostgreSQL/In-Memory
            entity.Property(u => u.Roles)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => string.IsNullOrEmpty(v)
                        ? new HashSet<Role>()
                        : JsonSerializer.Deserialize<HashSet<Role>>(v, (JsonSerializerOptions?)null) ?? new HashSet<Role>()
                );
        });

        // ReportModel table configuration
        modelBuilder.Entity<ReportModel>(entity =>
        {
            entity.HasKey(r => r.Id);

            // Store List<string> ImagePaths as JSON string in PostgreSQL/In-Memory
            entity.Property(r => r.ImagePaths)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => string.IsNullOrEmpty(v)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                );

            entity.HasOne(r => r.User)
                .WithMany(u => u.Reports)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DemoModel table configuration
        modelBuilder.Entity<DemoModel>(entity =>
        {
            entity.HasKey(d => d.Id);
        });
    }
}
