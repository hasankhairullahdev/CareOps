using BillingService.Domain.Entities;
using BillingService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Persistence;

public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options) { }

    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<BillLineItem> BillLineItems => Set<BillLineItem>();

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
    }
}
