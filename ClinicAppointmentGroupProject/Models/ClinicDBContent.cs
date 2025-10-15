using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ClinicAppointmentGroupProject.Models
{
    // Inherit from DbContext to control which tables are mapped
    public class ClinicDbContext : DbContext
    {
        public ClinicDbContext(DbContextOptions<ClinicDbContext> options)
            : base(options)
        {
        }

        // --- DbSets for the FOUR required tables ---
        public DbSet<Patient> Patients { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<IdentityRole> Roles { get; set; }
        public DbSet<IdentityUserRole<string>> UserRoles { get; set; }

        // CRITICAL FIX: Add these DbSets back to satisfy the CustomUserStore constructor
        // These will NOT be created in the database because they are not configured below.
        public DbSet<IdentityUserClaim<string>> UserClaims { get; set; }
        public DbSet<IdentityUserLogin<string>> UserLogins { get; set; }
        public DbSet<IdentityUserToken<string>> UserTokens { get; set; }
        public DbSet<IdentityRoleClaim<string>> RoleClaims { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            // CRITICAL FIX: Manually configure keys for ALL Identity join tables for design-time checks
            // This prevents the 'requires a primary key' error during migration creation.
            builder.Entity<IdentityUserLogin<string>>().HasKey(l => new { l.LoginProvider, l.ProviderKey });
            builder.Entity<IdentityUserToken<string>>().HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
            builder.Entity<IdentityUserClaim<string>>().HasKey(c => c.Id);
            builder.Entity<IdentityRoleClaim<string>>().HasKey(rc => rc.Id);


            // 1. Manually configure Identity User table (aspnetusers)
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("aspnetusers");
                entity.HasKey(e => e.Id);

                // Fields to ignore (removed columns)
                entity.Ignore(e => e.PhoneNumber);
                entity.Ignore(e => e.PhoneNumberConfirmed);
                entity.Ignore(e => e.TwoFactorEnabled);
                entity.Ignore(e => e.LockoutEnabled);
                entity.Ignore(e => e.AccessFailedCount);
            });

            // 2. Manually configure Identity Role table (aspnetroles)
            builder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable("aspnetroles");
                entity.HasKey(e => e.Id);
            });

            // 3. Manually configure Identity UserRole join table (aspnetuserroles)
            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("aspnetuserroles");
                entity.HasKey(e => new { e.UserId, e.RoleId });
            });


            // --- Existing Patient Foreign Key Configuration ---

            // Patient -> Doctor
            builder.Entity<Patient>()
                .HasOne(p => p.Doctor)
                .WithMany()
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Patient -> ClientUser
            builder.Entity<Patient>()
                .HasOne(p => p.ClientUser)
                .WithMany()
                .HasForeignKey(p => p.ClientUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}