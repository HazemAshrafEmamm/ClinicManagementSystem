using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
    public class ReceptionistPortalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReceptionistPortalController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ReceptionistPortal
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        // GET: ReceptionistPortal/ConfirmAppointment/5
        public async Task<IActionResult> ConfirmAppointment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Default to a reasonable time (e.g. 5 PM) if time is midnight
            if (appointment.AppointmentDate.TimeOfDay == TimeSpan.Zero)
            {
                appointment.AppointmentDate = appointment.AppointmentDate.Date.AddHours(17);
            }

            // Get Doctor Schedule for that day
            var dayOfWeek = appointment.AppointmentDate.DayOfWeek;
            var doctorSchedule = await _context.DoctorSchedules
                .FirstOrDefaultAsync(s => s.DoctorId == appointment.DoctorId && s.DayOfWeek == dayOfWeek);
            
            ViewBag.DoctorSchedule = doctorSchedule;

            // Get existing appointments for that doctor on that day
            var existingAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == appointment.DoctorId 
                         && a.AppointmentDate.Date == appointment.AppointmentDate.Date 
                         && a.Id != appointment.Id
                         && a.Status != AppointmentStatus.Cancelled)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();
            
            ViewBag.ExistingAppointments = existingAppointments;

            return View(appointment);
        }

        // POST: ReceptionistPortal/ConfirmAppointment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmAppointment(int id, [Bind("Id,AppointmentDate,IsPaid,PaymentMethod")] Appointment editedAppointment)
        {
            if (id != editedAppointment.Id)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            try
            {
                appointment.AppointmentDate = editedAppointment.AppointmentDate;
                appointment.IsPaid = editedAppointment.IsPaid;
                appointment.PaymentMethod = editedAppointment.PaymentMethod;
                appointment.Status = AppointmentStatus.Confirmed;

                _context.Update(appointment);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "تم تأكيد الموعد بنجاح وتحديد الوقت.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "حدث خطأ أثناء حفظ التعديلات.");
            }
            
            // If we got this far, something failed, redisplay form
            appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);
            return View(appointment);
        }
        
        // POST: ReceptionistPortal/CompleteAppointment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Completed;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تعيين الموعد كمنتهي.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: ReceptionistPortal/CancelAppointment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Cancelled;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إلغاء الموعد.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: ReceptionistPortal/DoctorSchedules
        public async Task<IActionResult> DoctorSchedules()
        {
            var doctors = await _context.Doctors
                .Include(d => d.Specialty)
                .Include(d => d.Schedules)
                .ToListAsync();
                
            return View(doctors);
        }
    }
}
