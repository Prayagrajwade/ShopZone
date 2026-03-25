namespace ShopAPI.Infrastructure.Persistence.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<CartItemEntity> CartItems { get; set; }
    public DbSet<OrderEntity> Orders { get; set; }
    public DbSet<OrderItemEntity> OrderItems { get; set; }
    public DbSet<OrderStatusLogEntity> OrderStatusLogs { get; set; }
    public DbSet<PaymentLogEntity> PaymentLogs { get; set; }
    public DbSet<TempleteEntity> EmailTemplete { get; set; }
    public DbSet<TopProductDto> TopProducts => Set<TopProductDto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<UserEntity>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Name).IsRequired().HasMaxLength(100);
            e.Property(u => u.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Role).IsRequired().HasMaxLength(20).HasDefaultValue("user");
            e.Property(u => u.IsActive).HasDefaultValue(true);
            e.Property(u => u.SecurityStamp).HasMaxLength(200);
        });

        // Product
        modelBuilder.Entity<ProductEntity>(e =>
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
        modelBuilder.Entity<CartItemEntity>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.Product).WithMany().HasForeignKey(c => c.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        // Order
        modelBuilder.Entity<OrderEntity>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(o => o.Status).IsRequired().HasMaxLength(50);
            e.HasOne(o => o.User).WithMany().HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(o => o.Items).WithOne().HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        // OrderItem
        modelBuilder.Entity<OrderItemEntity>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
            e.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderStatusLogEntity>(e =>
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

        modelBuilder.Entity<PaymentLogEntity>(e =>
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

        modelBuilder.Entity<TempleteEntity>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Subject).IsRequired().HasMaxLength(500);
            e.Property(p => p.Body).HasMaxLength(2000);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.Property(p => p.Type).HasMaxLength(100);
        });

        modelBuilder.Entity<TopProductDto>(e =>
        {
            e.HasNoKey();
            e.ToView(null);
        });
    }
}
