using Microsoft.EntityFrameworkCore;
using PatientService.Domain.Entities;

namespace PatientService.Infrastructure.Persistence;

public class PatientDbContext : DbContext
{
    public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<BloodType> BloodTypes => Set<BloodType>();
    public DbSet<AllergyType> AllergyTypes => Set<AllergyType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DateOfBirth).IsRequired();
            entity.Property(e => e.Gender).IsRequired().HasMaxLength(10);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.MedicalRecordNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.MedicalRecordNumber).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<BloodType>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Name).IsRequired().HasMaxLength(10);
            e.HasIndex(x => x.Name).IsUnique();
            // Seed all standard blood types
            e.HasData(
                new BloodType(1, "A+"), new BloodType(2, "A-"),
                new BloodType(3, "B+"), new BloodType(4, "B-"),
                new BloodType(5, "AB+"), new BloodType(6, "AB-"),
                new BloodType(7, "O+"), new BloodType(8, "O-")
            );
        });

        modelBuilder.Entity<AllergyType>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Description).HasMaxLength(300);
            e.HasIndex(x => x.Name).IsUnique();
        });
        // Seed data is handled via migration (InitialCreate)
    }
}
