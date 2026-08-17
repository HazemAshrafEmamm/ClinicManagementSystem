using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicManagementSystem.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientPortalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PatientPortalController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: PatientPortal/Index (My Appointments)
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return NotFound("لم يتم العثور على بيانات المريض الخاصة بك.");
            }

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .ThenInclude(d => d.Specialty)
                .Where(a => a.PatientId == patient.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            ViewBag.Receptionists = await _context.Receptionists.ToListAsync();

            return View(appointments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null) return Unauthorized();

            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.PatientId == patient.Id);
            
            if (appointment != null && appointment.Status == AppointmentStatus.Pending)
            {
                appointment.Status = AppointmentStatus.Cancelled;
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم إلغاء الموعد بنجاح.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: PatientPortal/BookAppointment
        public async Task<IActionResult> BookAppointment()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return NotFound("لم يتم العثور على بيانات المريض الخاصة بك.");
            }

            ViewData["Specialties"] = new SelectList(_context.Specialties, "Id", "Name");
            // Initially empty doctors list, loaded via AJAX
            ViewData["Doctors"] = new SelectList(new List<Doctor>(), "Id", "FullName");

            var model = new AppointmentFormViewModel
            {
                PatientId = patient.Id,
                AppointmentDate = DateTime.Now.AddDays(1) // Default to tomorrow
            };

            return View(model);
        }

        // POST: PatientPortal/BookAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(AppointmentFormViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null || model.PatientId != patient.Id)
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                if (model.AppointmentDate.Date < DateTime.Now.Date)
                {
                    ModelState.AddModelError("AppointmentDate", "لا يمكن حجز موعد في الماضي.");
                }
                else if (!await DoctorWorksOnDay(model.DoctorId, model.AppointmentDate.DayOfWeek))
                {
                    ModelState.AddModelError("AppointmentDate", "الطبيب لا يعمل في هذا اليوم. يرجى اختيار يوم آخر من أيام عمل الطبيب.");
                }
                else
                {
                    var appointment = new Appointment
                    {
                        PatientId = model.PatientId,
                        DoctorId = model.DoctorId,
                        AppointmentDate = model.AppointmentDate.Date, // Only save the date initially
                        Duration = 30, // Default estimate
                        Notes = model.Notes,
                        Status = AppointmentStatus.Pending
                    };

                    _context.Add(appointment);
                    await _context.SaveChangesAsync();
                    
                    return RedirectToAction(nameof(BookingSuccess));
                }
            }

            var doctor = await _context.Doctors.FindAsync(model.DoctorId);
            ViewData["Specialties"] = new SelectList(_context.Specialties, "Id", "Name", doctor?.SpecialtyId);
            ViewData["Doctors"] = new SelectList(_context.Doctors.Where(d => d.SpecialtyId == doctor.SpecialtyId), "Id", "FullName", model.DoctorId);
            return View(model);
        }

        // GET: PatientPortal/BookingSuccess
        public async Task<IActionResult> BookingSuccess()
        {
            var receptionists = await _context.Receptionists.ToListAsync();
            return View(receptionists);
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctorsBySpecialty(int specialtyId)
        {
            var doctors = await _context.Doctors
                .Where(d => d.SpecialtyId == specialtyId)
                .Select(d => new { id = d.Id, fullName = d.FullName })
                .ToListAsync();
            return Json(doctors);
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctorSchedules(int doctorId)
        {
            var schedulesList = await _context.DoctorSchedules
                .Where(s => s.DoctorId == doctorId)
                .ToListAsync();

            var schedules = schedulesList
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .Select(s => new {
                    dayName = s.DayOfWeek switch {
                        DayOfWeek.Sunday => "الأحد",
                        DayOfWeek.Monday => "الإثنين",
                        DayOfWeek.Tuesday => "الثلاثاء",
                        DayOfWeek.Wednesday => "الأربعاء",
                        DayOfWeek.Thursday => "الخميس",
                        DayOfWeek.Friday => "الجمعة",
                        DayOfWeek.Saturday => "السبت",
                        _ => ""
                    },
                    startTime = s.StartTime.ToString(@"hh\:mm"),
                    endTime = s.EndTime.ToString(@"hh\:mm")
                })
                .ToList();

            return Json(schedules);
        }

        private async Task<bool> DoctorWorksOnDay(int doctorId, DayOfWeek day)
        {
            return await _context.DoctorSchedules.AnyAsync(s => s.DoctorId == doctorId && s.DayOfWeek == day);
        }
    }
}
