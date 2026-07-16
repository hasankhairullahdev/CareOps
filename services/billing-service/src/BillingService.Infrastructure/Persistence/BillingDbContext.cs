using BillingService.Domain.Entities;
using BillingService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Persistence;

public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options) { }

    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<BillLineItem> BillLineItems => Set<BillLineItem>();
    public DbSet<ServiceTariff> ServiceTariffs => Set<ServiceTariff>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bill>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Status).HasConversion<string>().IsRequired();
            e.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            e.HasIndex(x => x.AppointmentId).IsUnique(); // one bill per appointment
            e.HasIndex(x => x.PatientId);
            e.HasMany(x => x.LineItems)
             .WithOne()
             .HasForeignKey(x => x.BillId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BillLineItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Description).IsRequired().HasMaxLength(500);
            e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<PaymentMethod>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Description).HasMaxLength(300);
            e.HasIndex(x => x.Name).IsUnique();
            // Seed common payment methods
            e.HasData(
                new { Id = new Guid("c0000000-0000-0000-0000-000000000001"), Name = "Cash", Description = (string?)null, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = new Guid("c0000000-0000-0000-0000-000000000002"), Name = "Transfer Bank", Description = (string?)null, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = new Guid("c0000000-0000-0000-0000-000000000003"), Name = "BPJS", Description = (string?)"Jaminan Kesehatan Nasional", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = new Guid("c0000000-0000-0000-0000-000000000004"), Name = "Kartu Debit/Kredit", Description = (string?)null, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        });

        modelBuilder.Entity<ServiceTariff>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.ServiceName).IsRequired().HasMaxLength(200);
            e.Property(x => x.Category).IsRequired().HasMaxLength(100);
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasIndex(x => new { x.Category, x.ServiceName });

            // Seed default consultation tariff
            e.HasData(new
            {
                Id = new Guid("b0000000-0000-0000-0000-000000000001"),
                ServiceName = "Biaya Konsultasi Umum",
                Category = "Consultation",
                Price = 150_000m,
                Description = (string?)null,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = (DateTime?)null,
            });
        });
    }
}
