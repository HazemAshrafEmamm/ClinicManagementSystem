using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Controllers
{
    [Authorize(Roles = "Admin, Receptionist")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Appointments (Doctor Schedules)
        public async Task<IActionResult> Index(int? filterDoctorId, DayOfWeek? filterDay)
        {
            var schedules = _context.DoctorSchedules
                .Include(s => s.Doctor)
                .AsQueryable();

            if (filterDoctorId.HasValue && filterDoctorId.Value > 0)
            {
                schedules = schedules.Where(s => s.DoctorId == filterDoctorId.Value);
            }

            if (filterDay.HasValue)
            {
                schedules = schedules.Where(s => s.DayOfWeek == filterDay.Value);
            }

            ViewData["Doctors"] = new SelectList(_context.Doctors, "Id", "FullName", filterDoctorId);
            ViewData["CurrentDayFilter"] = filterDay;
            
            // Order locally because of SQLite limitations with enums/timespans if any, 
            // but for DoctorId it's fine. We will order by DoctorName, then DayOfWeek, then StartTime.
            var schedulesList = await schedules.ToListAsync();

            var sortedSchedules = schedulesList
                .OrderBy(s => s.Doctor?.FullName)
                .ThenBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToList();

            return View(sortedSchedules);
        }
    }
}
