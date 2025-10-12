using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentGroupProject.Models
{
    public class Patient
    {

        [Key]
        public required string appointmentId { get; set; } 

        public required string patientName { get; set; }
        public required string department { get; set; }

        public DateOnly AppointmentDate { get; set; }

    }
}
