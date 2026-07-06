using PharmacyService.Domain.Entities;
using PharmacyService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace PharmacyService.Infrastructure.Persistence;

public class PharmacyDbContext : DbContext
{
    public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options) : base(options) { }

    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Medicine>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.GenericName).IsRequired().HasMaxLength(200);
            e.Property(x => x.Category).IsRequired().HasMaxLength(100);
            e.Property(x => x.Unit).IsRequired().HasMaxLength(20);
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.Property(x => x.ExpiryDate).IsRequired();
        });

        modelBuilder.Entity<Prescription>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Status).HasConversion<string>().IsRequired();
            e.HasIndex(x => x.ExternalPrescriptionId).IsUnique();
            e.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.PrescriptionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PrescriptionItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.MedicineName).IsRequired().HasMaxLength(200);
            e.Property(x => x.Dosage).IsRequired().HasMaxLength(100);
            e.Property(x => x.Instructions).HasMaxLength(500);
        });

        modelBuilder.Entity<StockMovement>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Type).HasConversion<string>().IsRequired();
            e.Property(x => x.Reason).HasMaxLength(500);
        });

        // Seed medicines
        modelBuilder.Entity<Medicine>().HasData(
            new { Id = new Guid("00000000-0000-0000-0000-000000000001"), Name = "Paracetamol 500mg", GenericName = "Paracetamol", Category = "Analgesic", Unit = "Tablet", StockQuantity = 500, MinimumStock = 50, Price = 500m, ExpiryDate = new DateOnly(2026, 12, 31) },
            new { Id = new Guid("00000000-0000-0000-0000-000000000002"), Name = "Amoxicillin 500mg", GenericName = "Amoxicillin", Category = "Antibiotic", Unit = "Kapsul", StockQuantity = 200, MinimumStock = 30, Price = 2500m, ExpiryDate = new DateOnly(2026, 6, 30) },
            new { Id = new Guid("00000000-0000-0000-0000-000000000003"), Name = "Omeprazole 20mg", GenericName = "Omeprazole", Category = "Antacid", Unit = "Kapsul", StockQuantity = 150, MinimumStock = 20, Price = 3000m, ExpiryDate = new DateOnly(2026, 9, 30) },
            new { Id = new Guid("00000000-0000-0000-0000-000000000004"), Name = "Cetirizine 10mg", GenericName = "Cetirizine", Category = "Antihistamine", Unit = "Tablet", StockQuantity = 10, MinimumStock = 20, Price = 1500m, ExpiryDate = new DateOnly(2025, 3, 31) },
            new { Id = new Guid("00000000-0000-0000-0000-000000000005"), Name = "Metformin 500mg", GenericName = "Metformin", Category = "Antidiabetic", Unit = "Tablet", StockQuantity = 300, MinimumStock = 50, Price = 1000m, ExpiryDate = new DateOnly(2026, 12, 31) }
        );
    }
}
