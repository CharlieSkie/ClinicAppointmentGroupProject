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

        // Add these new properties for approval system
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
        public string? AdminNotes { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
    }

    public enum UserType
    {
        Admin,
        Doctor,
        Client
    }

    // Add this new enum for approval status
    public enum ApprovalStatus
    {
        Pending,
        Approved,
        Rejected
    }
}