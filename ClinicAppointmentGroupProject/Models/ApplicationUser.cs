using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentGroupProject.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public UserType UserType { get; set; }

        public string? Specialization { get; set; }
        public string? LicenseNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public enum UserType
    {
        Admin,
        Doctor,
        Client
    }
}