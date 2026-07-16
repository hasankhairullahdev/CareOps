using AppointmentService.Domain.Entities;
using AppointmentService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Infrastructure.Persistence;

public class AppointmentDbContext : DbContext
{
    public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options) : base(options) { }

    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.PatientId).IsRequired();
            entity.Property(e => e.DoctorId).IsRequired();
            entity.Property(e => e.ScheduledAt).IsRequired();
            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasOne(e => e.Doctor)
                  .WithMany()
                  .HasForeignKey(e => e.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.DoctorId, e.ScheduledAt });
            entity.HasIndex(e => e.PatientId);
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Specialization).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.LicenseNumber).IsUnique();
            entity.Property(e => e.Schedule).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasMany(e => e.Items)
                  .WithOne()
                  .HasForeignKey(e => e.PrescriptionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PrescriptionItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.MedicineName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Dosage).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Instructions).HasMaxLength(500);
        });

        // Seed doctors
        modelBuilder.Entity<Doctor>().HasData(
            new { Id = new Guid("d0000000-0000-0000-0000-000000000001"), Name = "Dr. Andi Wirawan", Specialization = "General Practice", LicenseNumber = "STR-001-2024", Schedule = "Mon-Fri 08:00-16:00", Phone = (string?)null, Email = (string?)null, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new { Id = new Guid("d0000000-0000-0000-0000-000000000002"), Name = "Dr. Sari Kusuma", Specialization = "Internal Medicine", LicenseNumber = "STR-002-2024", Schedule = "Mon-Fri 09:00-17:00", Phone = (string?)null, Email = (string?)null, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new { Id = new Guid("d0000000-0000-0000-0000-000000000003"), Name = "Dr. Bima Prasetyo", Specialization = "Pediatrics", LicenseNumber = "STR-003-2024", Schedule = "Tue-Sat 08:00-15:00", Phone = (string?)null, Email = (string?)null, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
