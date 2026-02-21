using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using LinksAndMore.Models;

namespace LinksAndMore.Data;

public class AppDbContext : DbContext
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<DashboardItem> Items { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DashboardItem>()
            .Property(e => e.VectorEmbedding)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => v == null ? null : JsonSerializer.Deserialize<float[]>(v, (JsonSerializerOptions?)null));
    }
}
