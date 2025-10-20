using Microsoft.EntityFrameworkCore;
using Flexischools.Domain.Entities;
using Flexischools.Domain.Enums;

namespace Flexischools.Infrastructure.Data;

public class FlexischoolsDbContext : DbContext
{
    public FlexischoolsDbContext(DbContextOptions<FlexischoolsDbContext> options) : base(options)
    {
    }

    public DbSet<Parent> Parents { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Canteen> Canteens { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Parent configuration
        modelBuilder.Entity<Parent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.WalletBalance).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Student configuration
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Allergens).HasMaxLength(200);
            entity.HasOne(e => e.Parent)
                  .WithMany(p => p.Students)
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Canteen configuration
        modelBuilder.Entity<Canteen>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.OpeningDays).HasMaxLength(200);
            entity.Property(e => e.OrderCutOffTime).HasMaxLength(5);
        });

        // MenuItem configuration
        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.AllergenTags).HasMaxLength(200);
            entity.HasOne(e => e.Canteen)
                  .WithMany(c => c.MenuItems)
                  .HasForeignKey(e => e.CanteenId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FulfilmentDate).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasConversion<int>();
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.IdempotencyKey).HasMaxLength(100);
            entity.HasIndex(e => e.IdempotencyKey).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL");
            
            entity.HasOne(e => e.Parent)
                  .WithMany(p => p.Orders)
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Student)
                  .WithMany(s => s.Orders)
                  .HasForeignKey(e => e.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Canteen)
                  .WithMany(c => c.Orders)
                  .HasForeignKey(e => e.CanteenId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).IsRequired();
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.MenuItem)
                  .WithMany(mi => mi.OrderItems)
                  .HasForeignKey(e => e.MenuItemId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Parents
        modelBuilder.Entity<Parent>().HasData(
            new Parent { Id = 1, Email = "john.smith@email.com", Name = "John Smith", WalletBalance = 50.00m },
            new Parent { Id = 2, Email = "sarah.jones@email.com", Name = "Sarah Jones", WalletBalance = 25.50m },
            new Parent { Id = 3, Email = "mike.wilson@email.com", Name = "Mike Wilson", WalletBalance = 100.00m }
        );

        // Seed Canteens
        modelBuilder.Entity<Canteen>().HasData(
            new Canteen { Id = 1, Name = "Primary School Canteen", OpeningDays = "Monday,Tuesday,Wednesday,Thursday,Friday", OrderCutOffTime = "09:30" },
            new Canteen { Id = 2, Name = "High School Canteen", OpeningDays = "Monday,Tuesday,Wednesday,Thursday,Friday", OrderCutOffTime = "08:45" },
            new Canteen { Id = 3, Name = "Sports Canteen", OpeningDays = "Monday,Wednesday,Friday", OrderCutOffTime = "10:00" }
        );

        // Seed Students
        modelBuilder.Entity<Student>().HasData(
            new Student { Id = 1, Name = "Emma Smith", ParentId = 1, Allergens = "nuts" },
            new Student { Id = 2, Name = "Liam Smith", ParentId = 1, Allergens = null },
            new Student { Id = 3, Name = "Sophie Jones", ParentId = 2, Allergens = "dairy,eggs" },
            new Student { Id = 4, Name = "Oliver Wilson", ParentId = 3, Allergens = "gluten" }
        );

        // Seed Menu Items
        modelBuilder.Entity<MenuItem>().HasData(
            new MenuItem { Id = 1, Name = "Chicken Sandwich", Description = "Fresh chicken breast with lettuce and mayo", Price = 6.50m, DailyStockCount = 20, AllergenTags = "gluten,dairy", CanteenId = 1 },
            new MenuItem { Id = 2, Name = "Veggie Wrap", Description = "Mixed vegetables in a tortilla wrap", Price = 5.00m, DailyStockCount = 15, AllergenTags = "gluten", CanteenId = 1 },
            new MenuItem { Id = 3, Name = "Fruit Salad", Description = "Seasonal fresh fruits", Price = 4.00m, DailyStockCount = 25, AllergenTags = null, CanteenId = 1 },
            new MenuItem { Id = 4, Name = "Pizza Slice", Description = "Margherita pizza slice", Price = 4.50m, DailyStockCount = 30, AllergenTags = "gluten,dairy", CanteenId = 2 },
            new MenuItem { Id = 5, Name = "Caesar Salad", Description = "Romaine lettuce with caesar dressing", Price = 7.00m, DailyStockCount = 12, AllergenTags = "dairy,eggs", CanteenId = 2 },
            new MenuItem { Id = 6, Name = "Energy Bar", Description = "Granola and nuts energy bar", Price = 3.50m, DailyStockCount = 40, AllergenTags = "nuts", CanteenId = 3 },
            new MenuItem { Id = 7, Name = "Smoothie", Description = "Mixed berry smoothie", Price = 5.50m, DailyStockCount = 20, AllergenTags = "dairy", CanteenId = 3 }
        );
    }
}
