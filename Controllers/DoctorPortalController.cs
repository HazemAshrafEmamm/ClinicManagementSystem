using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicManagementSystem.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorPortalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorPortalController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var doctorEmail = User.Identity?.Name;
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == doctorEmail);

            if (doctor == null)
            {
                return NotFound("لم يتم العثور على بيانات الطبيب الخاصة بك في النظام.");
            }

            var scheduledDays = await _context.DoctorSchedules
                .Where(s => s.DoctorId == doctor.Id)
                .Select(s => s.DayOfWeek)
                .Distinct()
                .ToListAsync();
                
            // Sort scheduled days from Sunday to Saturday (0 to 6)
            scheduledDays = scheduledDays.OrderBy(d => (int)d).ToList();

            var confirmedCount = await _context.Appointments
                .CountAsync(a => a.DoctorId == doctor.Id && a.Status == AppointmentStatus.Confirmed);

            var completedCount = await _context.Appointments
                .CountAsync(a => a.DoctorId == doctor.Id && a.Status == AppointmentStatus.Completed);

            var model = new ClinicManagementSystem.Models.ViewModels.DoctorDashboardViewModel
            {
                ConfirmedAppointmentsCount = confirmedCount,
                CompletedAppointmentsCount = completedCount,
                ScheduledDays = scheduledDays
            };
            
            // Only fetch non-cancelled appointments from today onwards to avoid loading all history into memory
            var upcomingAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.Id && a.Status != AppointmentStatus.Cancelled && a.AppointmentDate >= DateTime.Today)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            // Populate appointments per scheduled day
            foreach (var day in scheduledDays)
            {
                var appointmentsForDay = upcomingAppointments
                    .Where(a => a.AppointmentDate.DayOfWeek == day)
                    .ToList();
                    
                model.AppointmentsByDay[day] = appointmentsForDay;
            }

            ViewBag.DoctorName = doctor.FullName;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, AppointmentStatus status, string? notes)
        {
            var doctorEmail = User.Identity?.Name;
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == doctorEmail);

            if (doctor == null) return Unauthorized();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null || appointment.DoctorId != doctor.Id)
            {
                return NotFound();
            }

            appointment.Status = status;
            if (!string.IsNullOrEmpty(notes))
            {
                appointment.Notes = notes;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم تحديث حالة الموعد بنجاح.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Schedule()
        {
            var doctorEmail = User.Identity?.Name;
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == doctorEmail);

            if (doctor == null) return NotFound("لم يتم العثور على بيانات الطبيب الخاصة بك.");

            var schedulesList = await _context.DoctorSchedules
                .Where(s => s.DoctorId == doctor.Id)
                .ToListAsync();

            var schedules = schedulesList
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToList();

            ViewBag.DoctorName = doctor.FullName;
            return View(schedules);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSchedule(DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        {
            var doctorEmail = User.Identity?.Name;
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == doctorEmail);
            if (doctor == null) return Unauthorized();

            if (startTime >= endTime)
            {
                TempData["Error"] = "وقت البدء يجب أن يكون قبل وقت الانتهاء.";
                return RedirectToAction(nameof(Schedule));
            }

            var existingSchedules = await _context.DoctorSchedules
                .Where(s => s.DoctorId == doctor.Id && s.DayOfWeek == dayOfWeek)
                .ToListAsync();

            var hasOverlap = existingSchedules.Any(s => 
                s.StartTime < endTime && s.EndTime > startTime);

            if (hasOverlap)
            {
                TempData["Error"] = "يوجد تعارض مع موعد عمل مسجل مسبقاً في هذا اليوم.";
                return RedirectToAction(nameof(Schedule));
            }

            var schedule = new DoctorSchedule
            {
                DoctorId = doctor.Id,
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime
            };

            _context.DoctorSchedules.Add(schedule);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "تم إضافة موعد العمل بنجاح.";
            return RedirectToAction(nameof(Schedule));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var doctorEmail = User.Identity?.Name;
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == doctorEmail);
            if (doctor == null) return Unauthorized();

            var schedule = await _context.DoctorSchedules.FindAsync(id);
            if (schedule != null && schedule.DoctorId == doctor.Id)
            {
                _context.DoctorSchedules.Remove(schedule);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف موعد العمل بنجاح.";
            }

            return RedirectToAction(nameof(Schedule));
        }
    }
}
