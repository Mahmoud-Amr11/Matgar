using Matgar.Domain.Entities;
using Matgar.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Matgar.Infrastructure.Persistence.Contexts
{
    public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<StockItem> StockItems => Set<StockItem>();
        public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<Coupon> Coupons => Set<Coupon>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ApplicationDbContext).Assembly);
        }
    }
}
