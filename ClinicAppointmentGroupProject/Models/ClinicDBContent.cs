using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClinicAppointmentGroupProject.Models
{
    public class ClinicDbContext : IdentityDbContext<ApplicationUser>
    {
        public ClinicDbContext(DbContextOptions<ClinicDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 🚀 NEW: Ignore unwanted columns on the ApplicationUser (AspNetUsers) table
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Ignore(e => e.PhoneNumber);
                entity.Ignore(e => e.PhoneNumberConfirmed);
                entity.Ignore(e => e.TwoFactorEnabled);
                entity.Ignore(e => e.LockoutEnabled);
                entity.Ignore(e => e.AccessFailedCount);
            });
            // End NEW

            // Patient -> Doctor (Foreign Key Configuration)
            builder.Entity<Patient>()
                .HasOne(p => p.Doctor)
                .WithMany()
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Patient -> ClientUser (Foreign Key Configuration)
            builder.Entity<Patient>()
                .HasOne(p => p.ClientUser)
                .WithMany()
                .HasForeignKey(p => p.ClientUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}