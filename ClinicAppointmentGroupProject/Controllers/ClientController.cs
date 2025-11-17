using ClinicAppointmentGroupProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentGroupProject.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientController : Controller
    {
        private readonly ClinicDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClientController(ClinicDbContext context, UserManager<ApplicationUser> userManager)
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

            var model = new ClientDashboardViewModel
            {
                UpcomingAppointments = await _context.Patients
                    .Where(p => p.ClientUserId == currentUser.Id && p.AppointmentDate >= DateTime.Today)
                    .Include(p => p.Doctor)
                    .OrderBy(p => p.AppointmentDate)
                    .Take(5)
                    .ToListAsync(),

                RecentAppointments = await _context.Patients
                    .Where(p => p.ClientUserId == currentUser.Id)
                    .Include(p => p.Doctor)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> BookAppointment()
        {
            var doctors = await _userManager.GetUsersInRoleAsync("Doctor");
            ViewBag.Doctors = doctors ?? new List<ApplicationUser>();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(BookAppointmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);

                // Null check for currentUser
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var appointment = new Patient
                {
                    appointmentId = GenerateNextId(),
                    patientName = model.PatientName ?? string.Empty,
                    department = model.Department ?? string.Empty,
                    AppointmentDate = model.AppointmentDate,
                    DoctorId = model.DoctorId,
                    ClientUserId = currentUser.Id,
                    Status = AppointmentStatus.Pending
                };

                _context.Patients.Add(appointment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Appointment booked successfully!";
                return RedirectToAction(nameof(MyAppointments));
            }

            var doctors = await _userManager.GetUsersInRoleAsync("Doctor");
            ViewBag.Doctors = doctors ?? new List<ApplicationUser>();
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
                .Where(p => p.ClientUserId == currentUser.Id)
                .Include(p => p.Doctor)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(appointments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(string appointmentId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var appointment = await _context.Patients
                .FirstOrDefaultAsync(p => p.appointmentId == appointmentId && p.ClientUserId == currentUser.Id);

            if (appointment != null)
            {
                _context.Patients.Remove(appointment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Appointment cancelled successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Appointment not found or you don't have permission to cancel it.";
            }

            return RedirectToAction(nameof(MyAppointments));
        }

        [HttpGet]
        public async Task<JsonResult> GetDoctorsByDepartment(string department)
        {
            var doctors = await _userManager.GetUsersInRoleAsync("Doctor");
            var filteredDoctors = doctors
                .Where(d => d.Specialization?.ToLower() == department?.ToLower())
                .Select(d => new { id = d.Id, name = $"Dr. {d.FullName} - {d.Specialization}" })
                .ToList();

            return Json(filteredDoctors);
        }

        private string GenerateNextId()
        {
            var lastRecord = _context.Patients
                .OrderByDescending(x => x.appointmentId)
                .FirstOrDefault();

            int nextNumber = 1;

            if (lastRecord != null && !string.IsNullOrEmpty(lastRecord.appointmentId))
            {
                var parts = lastRecord.appointmentId.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"BF00-{nextNumber}";
        }
    }

    public class ClientDashboardViewModel
    {
        public IList<Patient> UpcomingAppointments { get; set; } = new List<Patient>();
        public IList<Patient> RecentAppointments { get; set; } = new List<Patient>();
    }

    public class BookAppointmentViewModel
    {
        [Required]
        [Display(Name = "Patient Name")]
        public string PatientName { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Appointment Date")]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [Display(Name = "Select Doctor")]
        public string DoctorId { get; set; } = string.Empty;
    }
}