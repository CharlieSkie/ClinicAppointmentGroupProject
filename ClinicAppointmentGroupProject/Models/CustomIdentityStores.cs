using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ClinicAppointmentGroupProject.Models; // CRITICAL: Ensure this using exists

namespace ClinicAppointmentGroupProject.Models
{
    // Custom Role Store
    public class CustomRoleStore : RoleStore<IdentityRole, ClinicDbContext, string>
    {
        public CustomRoleStore(ClinicDbContext context, IdentityErrorDescriber? describer = null)
            : base(context, describer)
        {
        }
    }

    // Custom User Store
    public class CustomUserStore : UserStore<
        ApplicationUser,
        IdentityRole,
        ClinicDbContext,
        string,
        IdentityUserClaim<string>,
        IdentityUserRole<string>,
        IdentityUserLogin<string>,
        IdentityUserToken<string>,
        IdentityRoleClaim<string>>
    {
        public CustomUserStore(ClinicDbContext context, IdentityErrorDescriber? describer = null)
            : base(context, describer)
        {
        }
    }
}