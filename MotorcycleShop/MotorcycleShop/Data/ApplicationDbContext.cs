using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MotorcycleShop.Models;
using MotorcycleShop.Models.Admin;

namespace MotorcycleShop.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Motorcycle> Motorcycles { get; set; }
        public DbSet<SparePart> SpareParts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Cart> Carts { get; set; }

        public DbSet<Coupon> Coupons { get; set; }

        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<AdminLog> AdminLogs { get; set; }

        public DbSet<Payment> Payments { get; set; }

       
        public DbSet<Report> Reports { get; set; }


        public DbSet<UserDiscount> UserDiscounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Motorcycle)
                .WithOne(m => m.Product)
                .HasForeignKey<Motorcycle>(m => m.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.SparePart)
                .WithOne(s => s.Product)
                .HasForeignKey<SparePart>(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Brand configuration
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cart configuration
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany(u => u.Carts)  // ApplicationUser phải có property Carts
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // CartItem configuration - CHỈ GIỮ 1 CẤU HÌNH
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);  // Restrict từ Product

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.User)
                .WithMany(u => u.Feedbacks)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserDiscount>()
              .HasIndex(ud => new { ud.UserId, ud.DiscountId })
              .IsUnique();
            // **THÊM CONFIGURATION CHO USERDISCOUNT**
            modelBuilder.Entity<UserDiscount>()
                .HasOne(ud => ud.User)
                .WithMany(u => u.UserDiscounts)
                .HasForeignKey(ud => ud.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserDiscount>()
                .HasOne(ud => ud.Discount)
                .WithMany(d => d.UserDiscounts)
                .HasForeignKey(ud => ud.DiscountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserDiscount>()
                .HasIndex(ud => new { ud.UserId, ud.DiscountId })
                .IsUnique();

            // Promotion configuration
            modelBuilder.Entity<Promotion>()
              .HasOne(p => p.Discount)
              .WithMany(d => d.Promotions)
              .HasForeignKey(p => p.DiscountId)
              .OnDelete(DeleteBehavior.SetNull);

            // AdminLog configuration
            modelBuilder.Entity<AdminLog>()
                .HasOne(l => l.Admin)
                .WithMany()
                .HasForeignKey(l => l.AdminId)
                .OnDelete(DeleteBehavior.Restrict);
            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Brands first
            modelBuilder.Entity<Brand>().HasData(
                new Brand
                {
                    BrandId = 1,
                    Name = "Honda",
                    Description = "Famous Japanese motorcycle manufacturer",
                    LogoUrl = "/images/brands/honda.png",
                    Country = "Japan",
                    EstablishedYear = new DateTime(1948, 1, 1)
                },
                new Brand
                {
                    BrandId = 2,
                    Name = "Yamaha",
                    Description = "Japanese sports motorcycle brand",
                    LogoUrl = "/images/brands/yamaha.png",
                    Country = "Japan",
                    EstablishedYear = new DateTime(1955, 1, 1)
                },
                new Brand
                {
                    BrandId = 3,
                    Name = "GS",
                    Description = "Trusted spare parts brand",
                    LogoUrl = "/images/brands/gs.png",
                    Country = "Vietnam",
                    EstablishedYear = new DateTime(1995, 1, 1)
                }
            );

            // Seed Products with BrandId
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = 1,
                    Name = "Honda Wave Alpha 110",
                    TypeProduct = ProductType.Motorcycle,
                    Price = 19000000,
                    Quantity = 10,
                    BrandId = 1, // Honda
                    BrandName = "Honda",
                    ImageUrl = "/images/wave-alpha.jpg",
                    Description = "Durable motorcycle with excellent fuel efficiency"
                },
                new Product
                {
                    ProductId = 2,
                    Name = "Yamaha Exciter 155",
                    TypeProduct = ProductType.Motorcycle,
                    Price = 52000000,
                    Quantity = 5,
                    BrandId = 2, // Yamaha
                    BrandName = "Yamaha",
                    ImageUrl = "/images/exciter-155.jpg",
                    Description = "Powerful and sporty manual clutch motorcycle"
                },
                new Product
                {
                    ProductId = 3,
                    Name = "GS Battery",
                    TypeProduct = ProductType.SparePart,
                    Price = 650000,
                    Quantity = 50,
                    BrandId = 3, // GS
                    BrandName = "GS",
                    ImageUrl = "/images/acquy-gs.jpg",
                    Description = "High-quality motorcycle battery"
                }
            );

            // Seed Motorcycle details
            modelBuilder.Entity<Motorcycle>().HasData(
                new Motorcycle
                {
                    MotorcycleId = 1,
                    ProductId = 1,
                    VehicleName = "Honda Wave Alpha 110",
                    TypeVehicle = VehicleType.Scooter,
                    Displacement = 110,
                    FuelCapacity = 3.7f,
                    Weight = 97,
                    Color = "Red, Black, Blue"
                },
                new Motorcycle
                {
                    MotorcycleId = 2,
                    ProductId = 2,
                    VehicleName = "Yamaha Exciter 155",
                    TypeVehicle = VehicleType.Sport,
                    Displacement = 155,
                    FuelCapacity = 4.2f,
                    Weight = 133,
                    Color = "Blue, Black, White"
                }
            );

            // Seed SparePart details
            modelBuilder.Entity<SparePart>().HasData(
                new SparePart
                {
                    SparePartId = 1,
                    ProductId = 3,
                    SpareName = "GS Battery",
                    Category = "Electrical",
                    CompatibleWith = "Most motorcycles"
                }
            );

            // Seed Discount
            modelBuilder.Entity<Discount>().HasData(
                new Discount
                {
                    DiscountId = 1,
                    DiscountName = "Grand Opening Discount",
                    Code = "OPENING20",
                    Description = "20% discount for first order",
                    DateStart = DateTime.Now,
                    DateEnd = DateTime.Now.AddMonths(3),
                    DiscountType = DiscountType.Percentage,
                    DiscountValue = 20,
                    DiscountValueType = DiscountValueType.AllProducts,
                    IsActive = true,
                    UsageLimit = 100
                }
            );

            // Seed promotion data
          modelBuilder.Entity<Promotion>().HasData(
          new Promotion
            {
                PromotionId = 1,
                Condition = "First Order Discount",
                Description = "Get 20% off on your first purchase",
                ImagePath = "/images/promotions/first-order.jpg",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(6),
                IsActive = true,
                DiscountId = 1,
                DisplayOrder = 1
            },
            new Promotion
            {
                PromotionId = 2,
                Condition = "Free Shipping Over 2M",
                Description = "Free shipping for orders over 2,000,000 VND",
                ImagePath = "/images/promotions/free-shipping.jpg",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(3),
                IsActive = true,
                DisplayOrder = 2
            }
        );
        }
    }
}