using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentGroupProject.Models
{
    public class Patient
    {
        [Key]
        public string appointmentId { get; set; } = string.Empty;

        [Required]
        public string patientName { get; set; } = string.Empty;

        [Required]
        public string department { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? DoctorId { get; set; }
        public virtual ApplicationUser? Doctor { get; set; }

        public string? ClientUserId { get; set; }
        public virtual ApplicationUser? ClientUser { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    }

    public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        Completed,
        Cancelled
    }
}