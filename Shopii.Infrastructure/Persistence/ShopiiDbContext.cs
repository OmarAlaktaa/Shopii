using Microsoft.EntityFrameworkCore;
using Shopii.Domain.Entities.Auditing;
using Shopii.Domain.Entities.Carts;
using Shopii.Domain.Entities.Orders;
using Shopii.Domain.Entities.Payments;
using Shopii.Domain.Entities.Products;
using Shopii.Domain.Entities.Users;

namespace Shopii.Infrastructure.Persistence;

public class ShopiiDbContext : DbContext
{
    public ShopiiDbContext(
        DbContextOptions<ShopiiDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Brand> Brands => Set<Brand>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    public DbSet<PaymentStatus> PaymentStatuses => Set<PaymentStatus>();

    public DbSet<Cart> Carts => Set<Cart>();

    public DbSet<CartItem> CartItems => Set<CartItem>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Discover entity configuration classes.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ShopiiDbContext).Assembly);

        modelBuilder.Entity<User>()
        .Property(u => u.Password)
        .HasColumnName("PasswordHash");

        modelBuilder.Entity<User>(entity =>
        {
            // Keep the EF model in sync with the hand-authored Users table so
            // that SQLite table rebuilds (needed for the Email collation)
            // preserve its defaults and check constraint.
            entity.ToTable("Users", table =>
                table.HasCheckConstraint(
                    "CK_Users_IsVerified",
                    "\"IsVerified\" IN (0, 1)"));

            entity.Property(u => u.IsVerified)
                .HasDefaultValue(false);

            entity.Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(u => u.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // A deleted user is soft-deleted rather than physically removed, so
            // rows that reference the user id keep their foreign key intact.
            // Soft-deleted users are hidden from all normal queries; use
            // IgnoreQueryFilters() to read them (e.g. admin/restore).
            entity.HasQueryFilter(u => u.DeletedAt == null);

            // SQLite compares text case-sensitively by default, so logins and
            // duplicate checks would treat "User@Example.com" and
            // "user@example.com" as different addresses. NOCASE makes the
            // comparison (and the unique index below) case-insensitive at the
            // column level.
            entity.Property(u => u.Email)
                .UseCollation("NOCASE");

            // Email uniqueness applies to live accounts only. The database uses a
            // partial unique index (WHERE DeletedAt IS NULL) instead of a plain
            // UNIQUE(Email), so a deleted user keeps its real email while the same
            // address is still available for a new registration.
            entity.HasIndex(u => u.Email)
                .HasDatabaseName("IX_Users_Email_Live")
                .IsUnique()
                .HasFilter("\"DeletedAt\" IS NULL");

            // Helper index for admin/restore queries over deleted users.
            entity.HasIndex(u => u.DeletedAt)
                .HasDatabaseName("IX_Users_DeletedAt");
        });

        // The existing SQLite database was authored separately from the domain
        // model, so the table and column names do not always line up. Map them
        // explicitly here to keep the entities in sync with the real schema
        // without altering the database.
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");

            // Stored as "Title" in the database.
            entity.Property(p => p.Name).HasColumnName("Title");

            // BaseEntity audit fields use different column names.
            entity.Property(p => p.UpdatedByUserId).HasColumnName("UpdatedBy");
            entity.Property(p => p.DeletedByUserId).HasColumnName("DeletedBy");

            // The Products table has no "CreatedBy" column.
            entity.Ignore(p => p.CreatedByUserId);

            // Soft-deleted products are hidden from all normal queries.
            // Use IgnoreQueryFilters() to read them (e.g. admin/restore).
            entity.HasQueryFilter(p => p.DeletedAt == null);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Product_Categories");

            // Stored as "Title" in the database.
            entity.Property(c => c.Name).HasColumnName("Title");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            // The lookup table is called "User_Roles" and stores the title
            // in a "Title" column.
            entity.ToTable("User_Roles");
            entity.Property(r => r.RoleTitle).HasColumnName("Title");
        });

        modelBuilder.Entity<ProductImage>().ToTable("Product_Images");

        modelBuilder.Entity<ProductVariant>().ToTable("Product_Variantes");

        // Orders / Payments / Carts: the same "authored separately" convention
        // as Products above — map the entities onto the pre-existing tables.

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");

            // Stored as "StatusId" in the database.
            entity.Property(o => o.OrderStatusId).HasColumnName("StatusId");

            // BaseEntity audit fields use different column names.
            entity.Property(o => o.CreatedAt).HasColumnName("OrderedAt");
            entity.Property(o => o.UpdatedByUserId).HasColumnName("UpdatedBy");
            entity.Property(o => o.DeletedByUserId).HasColumnName("DeletedBy");

            // The Orders table has no "CreatedBy" column.
            entity.Ignore(o => o.CreatedByUserId);

            // Cancelled/archived orders are hidden from all normal queries.
            // Use IgnoreQueryFilters() to read them.
            entity.HasQueryFilter(o => o.DeletedAt == null);

            entity.HasOne(o => o.Status)
                .WithMany()
                .HasForeignKey(o => o.OrderStatusId);

            entity.HasOne(o => o.Payment)
                .WithMany()
                .HasForeignKey(o => o.PaymentId);

            // Deleting an order removes its line items with it. The interceptor
            // rewrites the order delete into a soft delete, so in practice the
            // items stay as history; a physical cascade only happens if the
            // order row is ever really removed.
            entity.HasMany(o => o.OrderItems)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("Order_Items");

            // Stored as "OneItemPurchasePrice" in the database.
            entity.Property(i => i.UnitPrice).HasColumnName("OneItemPurchasePrice");

            entity.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId);
        });

        modelBuilder.Entity<OrderStatus>().ToTable("Order_Statuses");

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");

            // BaseEntity audit fields use different column names.
            entity.Property(p => p.CreatedByUserId).HasColumnName("CreatedBy");

            // The Payments table only carries CreatedAt/CreatedBy; the remaining
            // audit columns from BaseEntity have no counterpart and are not
            // persisted.
            entity.Ignore(p => p.UpdatedAt);
            entity.Ignore(p => p.UpdatedByUserId);
            entity.Ignore(p => p.DeletedAt);
            entity.Ignore(p => p.DeletedByUserId);

            entity.HasOne(p => p.PaymentMethod)
                .WithMany()
                .HasForeignKey(p => p.PaymentMethodId);

            entity.HasOne(p => p.Status)
                .WithMany()
                .HasForeignKey(p => p.PaymentStatusId);
        });

        modelBuilder.Entity<PaymentMethod>().ToTable("Payment_Methods");

        modelBuilder.Entity<PaymentStatus>().ToTable("Payment_Statuses");

        modelBuilder.Entity<Cart>(entity =>
        {
            // BaseEntity-style audit columns with different names in the
            // database.
            entity.HasMany(c => c.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.ToTable("Cart_Items");

            // Stored as "CartItemId" in the database.
            entity.Property(i => i.Item).HasColumnName("CartItemId");

            entity.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");

            entity.HasKey(a => a.AuditLogId);

            entity.Property(a => a.AuditLogId)
                .ValueGeneratedOnAdd();

            entity.Property(a => a.Action)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(a => a.EntityType)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(a => a.EntityId)
                .HasMaxLength(100);

            entity.Property(a => a.ActorUserId)
                .HasMaxLength(100);

            entity.Property(a => a.CorrelationId)
                .HasMaxLength(128);

            entity.Property(a => a.Details)
                .HasMaxLength(4000);

            entity.HasIndex(a => a.Timestamp)
                .HasDatabaseName("IX_AuditLogs_Timestamp");

            entity.HasIndex(a => a.ActorUserId)
                .HasDatabaseName("IX_AuditLogs_ActorUserId");

            entity.HasIndex(a => new { a.EntityType, a.EntityId })
                .HasDatabaseName("IX_AuditLogs_EntityType_EntityId");

            entity.HasIndex(a => a.CorrelationId)
                .HasDatabaseName("IX_AuditLogs_CorrelationId");
        });
    }
}
