using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Stock> Stocks { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Category>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                e.Property(x => x.Description)
                    .HasMaxLength(500);
            });

            builder.Entity<Product>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(200);
                e.Property(x => x.Description)
                    .HasMaxLength(1000);
                e.Property(x => x.Price)
                    .HasColumnType("decimal(18,2)");

                e.HasOne(x => x.Category)
                    .WithMany()
                    .HasForeignKey(x => x.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Stock>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Quantity)
                    .IsRequired();
                e.Property(x => x.LastUpdated)
                    .IsRequired();

                e.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
