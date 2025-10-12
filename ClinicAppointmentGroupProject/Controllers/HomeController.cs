using ClinicAppointmentGroupProject.Models;

using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ClinicAppointmentGroupProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ClinicDbContext _context;

        public HomeController(ClinicDbContext context)
        {
            _context = context;
        }

        // ----------------- INDEX -----------------
        public IActionResult Index()
        {
            return View();
        }

        // ----------------- ABOUT -----------------
        public IActionResult About()
        {
            return View();
        }

        // ----------------- RESULT (Appointment List + Search) -----------------
        public IActionResult Result(string search)
        {
            var query = _context.Patient.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.patientName.Contains(search) ||
                    p.department.Contains(search));
            }

            var dataList = query.ToList();
            return View(dataList);
        }

        // ----------------- CREATE -----------------
        [HttpGet]
        public IActionResult Create()
        {
            var newData = new Patient
            {
                appointmentId = GenerateNextId(),
                patientName = "",
                department = ""

            };
            return View(newData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Patient data)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(data.appointmentId))
                    data.appointmentId = GenerateNextId();

                _context.Patient.Add(data);
                _context.SaveChanges();
                return RedirectToAction(nameof(Result));
            }

            return View(data);
        }

        // ----------------- EDIT -----------------
        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (id == null) return NotFound();

            var record = _context.Patient.FirstOrDefault(x => x.appointmentId == id);
            if (record == null) return NotFound();

            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, Patient data)
        {
            if (id != data.appointmentId) return BadRequest();

            if (ModelState.IsValid)
            {
                _context.Update(data);
                _context.SaveChanges();
                return RedirectToAction(nameof(Result));
            }

            return View(data);
        }

        // ----------------- DELETE -----------------
        [HttpGet]
        public IActionResult Delete(string id)
        {
            if (id == null) return NotFound();

            var record = _context.Patient.FirstOrDefault(x => x.appointmentId == id);
            if (record == null) return NotFound();

            return View(record);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            var record = _context.Patient.FirstOrDefault(x => x.appointmentId == id);
            if (record != null)
            {
                _context.Patient.Remove(record);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Result));
        }

        // ----------------- ID GENERATOR -----------------
        private string GenerateNextId()
        {
            var lastRecord = _context.Patient
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
}
