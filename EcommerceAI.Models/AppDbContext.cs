using Microsoft.EntityFrameworkCore;
using EcommerceAI.Models.Entities;

namespace EcommerceAI.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<UserActivity> UserActivities => Set<UserActivity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ─── User ────────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("idx_users_email");
            entity.Property(e => e.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(20).HasDefaultValue("Customer");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ─── Category ────────────────────────────────────────
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Slug).IsUnique().HasDatabaseName("idx_categories_slug");
        });

        // ─── Product ─────────────────────────────────────────
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.Stock).IsRequired();
            entity.Property(e => e.TagsJson).HasColumnName("Tags").HasColumnType("nvarchar(max)").HasDefaultValue("[]");
            entity.Ignore(e => e.Tags); // Computed property — not mapped to DB
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.CategoryId).HasDatabaseName("idx_products_category");
            entity.HasIndex(e => e.Price).HasDatabaseName("idx_products_price");
        });

        // ─── Order ───────────────────────────────────────────
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Status).HasMaxLength(30).HasDefaultValue("Pending");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId).HasDatabaseName("idx_orders_user");
        });

        // ─── OrderItem ──────────────────────────────────────
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.Quantity).IsRequired();

            entity.HasOne(e => e.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── Cart ────────────────────────────────────────────
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId).IsUnique().HasDatabaseName("idx_carts_user");
        });

        // ─── CartItem ───────────────────────────────────────
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Quantity).IsRequired();

            entity.HasOne(e => e.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(e => e.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── UserActivity ───────────────────────────────────
        modelBuilder.Entity<UserActivity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.ActivityType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Score).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Activities)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Activities)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.ProductId })
                .HasDatabaseName("idx_useractivity_user_product");
        });

        // ─── Seed Data ──────────────────────────────────────
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var electronicsId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        var clothingId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");
        var shoesId = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012");
        var booksId = Guid.Parse("d4e5f6a7-b8c9-0123-defa-234567890123");
        var homeId = Guid.Parse("e5f6a7b8-c9d0-1234-efab-345678901234");

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = electronicsId, Name = "Electronics", Slug = "electronics" },
            new Category { Id = clothingId, Name = "Clothing", Slug = "clothing" },
            new Category { Id = shoesId, Name = "Shoes", Slug = "shoes" },
            new Category { Id = booksId, Name = "Books", Slug = "books" },
            new Category { Id = homeId, Name = "Home & Kitchen", Slug = "home-kitchen" }
        );
    }
}
