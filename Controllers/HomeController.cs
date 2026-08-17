using System.Diagnostics;
using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (User.IsInRole("Patient"))
        {
            return RedirectToAction("Index", "PatientPortal");
        }
        else if (User.IsInRole("Doctor"))
        {
            return RedirectToAction("Index", "DoctorPortal");
        }
        else if (User.IsInRole("Receptionist"))
        {
            return RedirectToAction("Index", "ReceptionistPortal");
        }
        else if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }
        var today = DateTime.Now.Date;
        var tomorrow = today.AddDays(1);

        var totalSpecialties = await _context.Specialties.CountAsync();
        var totalDoctors = await _context.Doctors.CountAsync();
        var totalPatients = await _context.Patients.CountAsync();
        var totalAppointments = await _context.Appointments.CountAsync();

        var todaysAppointments = await _context.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();

        var todayCount = todaysAppointments.Count;
        var todayPending = todaysAppointments.Count(a => a.Status == AppointmentStatus.Pending);
        var todayConfirmed = todaysAppointments.Count(a => a.Status == AppointmentStatus.Confirmed);
        var todayCompleted = todaysAppointments.Count(a => a.Status == AppointmentStatus.Completed);
        var todayCancelled = todaysAppointments.Count(a => a.Status == AppointmentStatus.Cancelled);

        ViewBag.TotalSpecialties = totalSpecialties;
        ViewBag.TotalDoctors = totalDoctors;
        ViewBag.TotalPatients = totalPatients;
        ViewBag.TotalAppointments = totalAppointments;
        
        ViewBag.TodayCount = todayCount;
        ViewBag.TodayPending = todayPending;
        ViewBag.TodayConfirmed = todayConfirmed;
        ViewBag.TodayCompleted = todayCompleted;
        ViewBag.TodayCancelled = todayCancelled;

        return View(todaysAppointments);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
