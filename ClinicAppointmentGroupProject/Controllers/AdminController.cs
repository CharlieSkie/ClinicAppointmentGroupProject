using ClinicAppointmentGroupProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentGroupProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ClinicDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ClinicDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                TotalAppointments = await _context.Patients.CountAsync(),
                TotalDoctors = await _userManager.GetUsersInRoleAsync("Doctor"),
                TotalClients = await _userManager.GetUsersInRoleAsync("Client"),
                RecentAppointments = await _context.Patients
                    .Include(p => p.Doctor)
                    .Include(p => p.ClientUser)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(10)
                    .ToListAsync(),
                PendingApprovalsCount = await _userManager.Users.CountAsync(u => u.ApprovalStatus == ApprovalStatus.Pending)
            };

            return View(model);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(new UserRoleViewModel
                {
                    User = user,
                    Roles = roles
                });
            }

            return View(userRoles);
        }

        [HttpGet]
        public IActionResult CreateDoctor()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctor(CreateDoctorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doctor = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    UserType = UserType.Doctor,
                    Specialization = model.Specialization,
                    LicenseNumber = model.LicenseNumber,
                    ApprovalStatus = ApprovalStatus.Approved // Auto-approve admin-created doctors
                };

                var result = await _userManager.CreateAsync(doctor, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(doctor, "Doctor");
                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Appointments()
        {
            var appointments = await _context.Patients
                .Include(p => p.Doctor)
                .Include(p => p.ClientUser)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(appointments);
        }

        [HttpGet]
        public async Task<IActionResult> PendingApprovals()
        {
            var pendingUsers = await _userManager.Users
                .Where(u => u.ApprovalStatus == ApprovalStatus.Pending)
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();

            var userRoles = new List<UserRoleViewModel>();
            foreach (var user in pendingUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(new UserRoleViewModel
                {
                    User = user,
                    Roles = roles
                });
            }

            return View(userRoles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveUser(string userId, string? adminNotes = null)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.ApprovalStatus = ApprovalStatus.Approved;
                user.ApprovedAt = DateTime.Now;
                user.ApprovedBy = User.Identity?.Name;
                user.AdminNotes = adminNotes;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"User {user.FullName} has been approved successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "User not found.";
            }

            return RedirectToAction(nameof(PendingApprovals));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectUser(string userId, string? adminNotes = null)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.ApprovalStatus = ApprovalStatus.Rejected;
                user.AdminNotes = adminNotes;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"User {user.FullName} has been rejected and removed from the list.";
            }
            else
            {
                TempData["ErrorMessage"] = "User not found.";
            }

            return RedirectToAction(nameof(PendingApprovals));
        }
    }

    public class AdminDashboardViewModel
    {
        public int TotalAppointments { get; set; }
        public IList<ApplicationUser> TotalDoctors { get; set; } = new List<ApplicationUser>();
        public IList<ApplicationUser> TotalClients { get; set; } = new List<ApplicationUser>();
        public IList<Patient> RecentAppointments { get; set; } = new List<Patient>();
        public int PendingApprovalsCount { get; set; }
    }

    public class UserRoleViewModel
    {
        public ApplicationUser User { get; set; } = new ApplicationUser();
        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class CreateDoctorViewModel
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Specialization { get; set; } = string.Empty;

        [Required]
        [Display(Name = "License Number")]
        public string LicenseNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}