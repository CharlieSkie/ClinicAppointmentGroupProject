using ClinicAppointmentGroupProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicAppointmentGroupProject.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly ClinicDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorController(ClinicDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // Null check for currentUser
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new DoctorDashboardViewModel
            {
                TodayAppointments = await _context.Patients
                    .Where(p => p.DoctorId == currentUser.Id && p.AppointmentDate.Date == DateTime.Today)
                    .Include(p => p.ClientUser)
                    .OrderBy(p => p.AppointmentDate)
                    .ToListAsync(),

                UpcomingAppointments = await _context.Patients
                    .Where(p => p.DoctorId == currentUser.Id && p.AppointmentDate > DateTime.Today)
                    .Include(p => p.ClientUser)
                    .OrderBy(p => p.AppointmentDate)
                    .Take(10)
                    .ToListAsync(),

                TotalAppointments = await _context.Patients
                    .CountAsync(p => p.DoctorId == currentUser.Id)
            };

            return View(model);
        }

        public async Task<IActionResult> MyAppointments()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // Null check for currentUser
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var appointments = await _context.Patients
                .Where(p => p.DoctorId == currentUser.Id)
                .Include(p => p.ClientUser)
                .OrderByDescending(p => p.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAppointmentStatus(string appointmentId, AppointmentStatus status)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // Null check for currentUser and appointmentId
            if (currentUser == null || string.IsNullOrEmpty(appointmentId))
            {
                return RedirectToAction("Login", "Account");
            }

            var appointment = await _context.Patients
                .FirstOrDefaultAsync(p => p.appointmentId == appointmentId && p.DoctorId == currentUser.Id);

            if (appointment != null)
            {
                appointment.Status = status;
                _context.Update(appointment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyAppointments));
        }
    }

    public class DoctorDashboardViewModel
    {
        public IList<Patient> TodayAppointments { get; set; } = new List<Patient>();
        public IList<Patient> UpcomingAppointments { get; set; } = new List<Patient>();
        public int TotalAppointments { get; set; }
    }
}