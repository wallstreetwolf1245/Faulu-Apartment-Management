using FauluApartmentAPI.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FauluApartmentAPI.Data;

/// <summary>
/// Application database context for Faulu Apartment Management System
/// </summary>
public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    #region DbSets

    public DbSet<Building> Buildings { get; set; } = null!;
    public DbSet<Unit> Units { get; set; } = null!;
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Lease> Leases { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<TransactionStatusLog> TransactionStatusLogs { get; set; } = null!;
    public DbSet<MaintenanceOrder> MaintenanceOrders { get; set; } = null!;
    public DbSet<ServiceRequest> ServiceRequests { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<PaymentReversal> PaymentReversals { get; set; } = null!;

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Building Configuration
        modelBuilder.Entity<Building>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.PropertyValue).HasPrecision(18, 2);
            entity.HasOne(e => e.Owner).WithMany(u => u.OwnedBuildings).HasForeignKey(e => e.OwnerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Unit Configuration
        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.UnitType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MonthlyRent).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Deposit).HasPrecision(18, 2);
            entity.Property(e => e.SquareFootage).HasPrecision(10, 2);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasOne(e => e.Building).WithMany(b => b.Units).HasForeignKey(e => e.BuildingId).OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => new { e.BuildingId, e.UnitNumber }).IsUnique();
        });

        // Tenant Configuration
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.IdentificationNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IdentificationType).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.IdentificationNumber).IsUnique();
            entity.HasIndex(e => e.Email);
        });

        // Lease Configuration
        modelBuilder.Entity<Lease>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MonthlyRent).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.DepositAmount).HasPrecision(18, 2);
            entity.Property(e => e.SecondaryDeposit).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.LeaseTermType).HasMaxLength(50);
            entity.Property(e => e.RenewalPolicy).HasMaxLength(50);
            entity.HasOne(e => e.Tenant).WithMany(t => t.Leases).HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Unit).WithMany(u => u.Leases).HasForeignKey(e => e.UnitId).OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Payment Configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.LateFees).HasPrecision(18, 2);
            entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.PaymentType).HasMaxLength(50);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.HasOne(e => e.Tenant).WithMany(t => t.Payments).HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Lease).WithMany(l => l.Payments).HasForeignKey(e => e.LeaseId).OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DueDate);
        });

        // TransactionStatusLog Configuration
        modelBuilder.Entity<TransactionStatusLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReceiptNumber).HasMaxLength(100);
            entity.Property(e => e.TransactionId).HasMaxLength(100);
            entity.Property(e => e.Msisdn).HasMaxLength(50);
            entity.Property(e => e.RawPayload).IsRequired();
            entity.HasOne(e => e.Payment).WithMany().HasForeignKey(e => e.PaymentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.PaymentId);
            entity.HasIndex(e => e.TransactionId);
        });

        // MaintenanceOrder Configuration
        modelBuilder.Entity<MaintenanceOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Priority).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.EstimatedCost).HasPrecision(18, 2);
            entity.Property(e => e.ActualCost).HasPrecision(18, 2);
            entity.HasOne(e => e.Building).WithMany(b => b.MaintenanceOrders).HasForeignKey(e => e.BuildingId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Unit).WithMany().HasForeignKey(e => e.UnitId).OnDelete(DeleteBehavior.NoAction);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Priority);
        });

        // PaymentReversal Configuration
        modelBuilder.Entity<PaymentReversal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.PaymentReferenceId).HasMaxLength(200);
            entity.HasOne(e => e.Payment).WithMany().HasForeignKey(e => e.PaymentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.RequestedAt);
        });

        // ServiceRequest Configuration
        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Priority).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasOne(e => e.Tenant).WithMany(t => t.ServiceRequests).HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Unit).WithMany().HasForeignKey(e => e.UnitId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.AssignedManager).WithMany(u => u.CreatedServiceRequests).HasForeignKey(e => e.AssignedManagerId).OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Priority);
        });

        // Notification Configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.NotificationType).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.RelatedEntityType).HasMaxLength(100);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.RecipientUserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.SentDate);
        });

        // Rename AspNetRoles to Roles, etc.
        modelBuilder.Entity<IdentityRole<int>>().ToTable("AspNetRoles");
        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("AspNetUserRoles");
        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("AspNetUserClaims");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("AspNetUserLogins");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("AspNetRoleClaims");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("AspNetUserTokens");
    }
}
