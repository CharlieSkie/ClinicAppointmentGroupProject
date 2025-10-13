using ClinicAppointmentGroupProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClinicAppointmentGroupProject.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (User.IsInRole("Doctor"))
                {
                    return RedirectToAction("Dashboard", "Doctor");
                }
                else if (User.IsInRole("Client"))
                {
                    return RedirectToAction("Dashboard", "Client");
                }
            }

            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}