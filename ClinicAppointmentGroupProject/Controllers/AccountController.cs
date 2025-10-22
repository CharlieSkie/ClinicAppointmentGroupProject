using ClinicAppointmentGroupProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentGroupProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Check if user exists and their approval status
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    if (user.ApprovalStatus == ApprovalStatus.Pending)
                    {
                        ModelState.AddModelError(string.Empty, "Your account is pending admin approval. Please wait for approval before logging in.");
                        return View(model);
                    }
                    else if (user.ApprovalStatus == ApprovalStatus.Rejected)
                    {
                        ModelState.AddModelError(string.Empty, "Your account registration has been rejected. Please contact administrator.");
                        return View(model);
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    if (user != null)
                    {
                        if (await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            return RedirectToAction("Dashboard", "Admin");
                        }
                        else if (await _userManager.IsInRoleAsync(user, "Doctor"))
                        {
                            return RedirectToAction("Dashboard", "Doctor");
                        }
                        else if (await _userManager.IsInRoleAsync(user, "Client"))
                        {
                            return RedirectToAction("Dashboard", "Client");
                        }
                    }

                    return LocalRedirect(returnUrl ?? "/");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Map SelectedRole to UserType
                var userType = model.SelectedRole switch
                {
                    "Admin" => UserType.Admin,
                    "Doctor" => UserType.Doctor,
                    "Client" => UserType.Client,
                    _ => UserType.Client
                };

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    UserType = userType,
                    Specialization = model.Specialization,
                    LicenseNumber = model.LicenseNumber,
                    ApprovalStatus = ApprovalStatus.Pending // New users need approval
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign to role based on selection
                    await _userManager.AddToRoleAsync(user, model.SelectedRole);

                    // Don't sign in automatically - wait for admin approval
                    TempData["SuccessMessage"] = "Registration successful! Please wait for admin approval before logging in.";
                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a role")]
        [Display(Name = "Register As")]
        public string SelectedRole { get; set; } = string.Empty;

        // Doctor-specific fields (optional for other roles)
        [Display(Name = "Specialization")]
        public string? Specialization { get; set; }

        [Display(Name = "License Number")]
        public string? LicenseNumber { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}