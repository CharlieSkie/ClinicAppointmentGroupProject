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

        public async Task<IActionResult> MyAppointments(string filter = "all")
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // Null check for currentUser
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            IQueryable<Patient> appointmentsQuery = _context.Patients
                .Where(p => p.DoctorId == currentUser.Id)
                .Include(p => p.ClientUser);

            // Calculate date ranges for filters
            var todayStart = DateTime.Today;
            var todayEnd = DateTime.Today.AddDays(1).AddTicks(-1);

            var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var weekEnd = weekStart.AddDays(7).AddTicks(-1);

            // Apply filters
            switch (filter.ToLower())
            {
                case "today":
                    appointmentsQuery = appointmentsQuery.Where(p => p.AppointmentDate >= todayStart && p.AppointmentDate <= todayEnd);
                    break;
                case "week":
                    appointmentsQuery = appointmentsQuery.Where(p => p.AppointmentDate >= weekStart && p.AppointmentDate <= weekEnd);
                    break;
                case "all":
                default:
                    // No additional filtering
                    break;
            }

            var appointments = await appointmentsQuery
                .OrderByDescending(p => p.AppointmentDate)
                .ToListAsync();

            // Get counts for each filter using the same date ranges
            var todayCount = await _context.Patients
                .CountAsync(p => p.DoctorId == currentUser.Id && p.AppointmentDate >= todayStart && p.AppointmentDate <= todayEnd);

            var weekCount = await _context.Patients
                .CountAsync(p => p.DoctorId == currentUser.Id && p.AppointmentDate >= weekStart && p.AppointmentDate <= weekEnd);

            var allCount = await _context.Patients
                .CountAsync(p => p.DoctorId == currentUser.Id);

            ViewBag.Filter = filter;
            ViewBag.TodayCount = todayCount;
            ViewBag.WeekCount = weekCount;
            ViewBag.AllCount = allCount;

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
                TempData["SuccessMessage"] = $"Appointment status updated to {status}";
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