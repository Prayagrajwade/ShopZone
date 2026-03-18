namespace ShopAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderStatusLog> OrderStatusLogs { get; set; }
    public DbSet<PaymentLog> PaymentLogs { get; set; }
    public DbSet<TempleteModel> EmailTemplete { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Name).IsRequired().HasMaxLength(100);
            e.Property(u => u.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Role).IsRequired().HasMaxLength(20).HasDefaultValue("user");
        });

        // Product
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).IsRequired().HasMaxLength(200);
            e.Property(p => p.Description).HasMaxLength(1000);
            e.Property(p => p.Price).HasColumnType("decimal(18,2)");
            e.Property(p => p.Category).HasMaxLength(100);
            e.Property(p => p.ImageUrl).HasMaxLength(500);
            e.Property(p => p.IsActive).HasDefaultValue(true);
        });

        // CartItem
        modelBuilder.Entity<CartItem>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.Product).WithMany().HasForeignKey(c => c.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        // Order
        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(o => o.Status).IsRequired().HasMaxLength(50);
            e.HasOne(o => o.User).WithMany().HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(o => o.Items).WithOne().HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        // OrderItem
        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
            e.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderStatusLog>(e =>
        {
            e.ToTable("OrderStatusLogs");

            e.HasKey(x => x.Id);

            e.Property(x => x.OldStatus)
                .IsRequired()
                .HasMaxLength(50);

            e.Property(x => x.NewStatus)
                .IsRequired()
                .HasMaxLength(50);

            e.Property(x => x.Note)
                .HasMaxLength(500);

            e.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            e.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.OrderId);
        });

        modelBuilder.Entity<PaymentLog>(e =>
        {
            e.ToTable("PaymentLogs");

            e.HasKey(x => x.Id);

            e.Property(x => x.StripeEventId)
                .IsRequired()
                .HasMaxLength(200);

            e.Property(x => x.EventType)
                .IsRequired()
                .HasMaxLength(100);

            e.Property(x => x.PaymentIntentId)
                .IsRequired()
                .HasMaxLength(200);

            e.Property(x => x.Status)
                .HasMaxLength(50);

            e.Property(x => x.RawJson)
                .IsRequired();

            e.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            e.HasOne(x => x.Order)
                .WithMany() 
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(x => x.StripeEventId).IsUnique();
            e.HasIndex(x => x.PaymentIntentId);
        });

        modelBuilder.Entity<TempleteModel>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Subject).IsRequired().HasMaxLength(500);
            e.Property(p => p.Body).HasMaxLength(2000);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.Property(p => p.Type).HasMaxLength(100);
        });

        // Seed admin user
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            Name = "Admin",
            Email = "admin@shop.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "admin",
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        // Seed sample products
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Wireless Headphones", Description = "Premium noise-cancelling wireless headphones with 30hr battery", Price = 79.99m, Stock = 50, Category = "Electronics", ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 2, Name = "Smart Watch", Description = "Feature-packed smartwatch with health tracking", Price = 149.99m, Stock = 30, Category = "Electronics", ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 3, Name = "Running Shoes", Description = "Lightweight performance running shoes", Price = 89.99m, Stock = 75, Category = "Footwear", ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 4, Name = "Coffee Maker", Description = "12-cup programmable coffee maker with thermal carafe", Price = 49.99m, Stock = 40, Category = "Kitchen", ImageUrl = "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=400", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 5, Name = "Leather Wallet", Description = "Slim genuine leather bifold wallet with RFID blocking", Price = 29.99m, Stock = 100, Category = "Accessories", ImageUrl = "https://images.unsplash.com/photo-1627123424574-724758594e93?w=400", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 6, Name = "Backpack", Description = "Durable 30L travel backpack with laptop compartment", Price = 59.99m, Stock = 60, Category = "Bags", ImageUrl = "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=400", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
