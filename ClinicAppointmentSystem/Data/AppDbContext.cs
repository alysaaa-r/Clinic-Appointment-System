// Data/AppDbContext.cs
using ClinicAppointmentSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicAppointmentSystem.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique emails
        modelBuilder.Entity<Admin>().HasIndex(a => a.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Patient>().HasIndex(p => p.Email);

        // FK User -> Patient
        modelBuilder.Entity<Patient>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // MySQL DB-driven audit defaults
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entity.ClrType))
            {
                var createdAt = entity.FindProperty(nameof(BaseEntity.CreatedAt));
                var updatedAt = entity.FindProperty(nameof(BaseEntity.UpdatedAt));

                if (createdAt is not null)
                {
                    createdAt.SetColumnType("datetime(6)");
                    createdAt.SetDefaultValueSql("CURRENT_TIMESTAMP(6)");

                }

                if (updatedAt is not null)
                {
                    updatedAt.SetColumnType("datetime");
                }
            }
        }

        // Seed a main admin (password: Admin@123)
        var mainAdmin = new Admin
        {
            Id = 1,
            FirstName = "Main",
            LastName = "Admin",
            Email = "main.admin@clinic.com",
            PasswordHash = "WbKboGWUFWpJLTSie2bgdw==:13/yyv8AHzbpetGFu4V5zll/C31XP4mYD9hEopCC+bY=",
            IsApproved = true,
            IsMainAdmin = true,
            CreatedBy = "seed",
            CreatedAt = new DateTime(2024, 01, 01) // fixed date
        };
        modelBuilder.Entity<Admin>().HasData(mainAdmin);
    }
}