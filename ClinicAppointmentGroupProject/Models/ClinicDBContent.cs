using Microsoft.EntityFrameworkCore;

namespace ClinicAppointmentGroupProject.Models
{
    public class ClinicDbContext : DbContext
    {
        public ClinicDbContext(DbContextOptions<ClinicDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patient { get; set; }
    }
}
