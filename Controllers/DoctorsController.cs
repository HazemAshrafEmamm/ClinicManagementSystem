using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using ClinicManagementSystem.Services;

namespace ClinicManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public DoctorsController(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        // GET: Doctors
        public async Task<IActionResult> Index(string searchString, int? specialtyId)
        {
            var doctors = _context.Doctors.Include(d => d.Specialty).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                doctors = doctors.Where(d => d.FullName.Contains(searchString));
            }

            if (specialtyId.HasValue && specialtyId.Value > 0)
            {
                doctors = doctors.Where(d => d.SpecialtyId == specialtyId.Value);
            }

            ViewData["Specialties"] = new SelectList(_context.Specialties, "Id", "Name", specialtyId);
            ViewData["CurrentSearch"] = searchString;

            return View(await doctors.ToListAsync());
        }

        // GET: Doctors/Create
        public IActionResult Create()
        {
            ViewData["Specialties"] = new SelectList(_context.Specialties, "Id", "Name");
            return View();
        }

        // POST: Doctors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,Phone,Email,SpecialtyId")] Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                // Create Identity User for the doctor using UserService
                var (succeeded, _, tempPassword, errors) = await _userService.CreateUserWithRoleAsync(doctor.Email, "Doctor", "Doc");
                
                if (succeeded)
                {
                    if (!string.IsNullOrEmpty(tempPassword))
                    {
                        TempData["Success"] = $"تم إضافة الطبيب بنجاح. كلمة المرور المؤقتة للحساب هي: {tempPassword} (يرجى إعطاؤها للطبيب لتسجيل الدخول).";
                    }
                    else
                    {
                        TempData["Success"] = "تم إضافة بيانات الطبيب. (البريد الإلكتروني لديه حساب بالفعل في النظام)";
                    }
                }
                else
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                    ViewData["Specialties"] = new SelectList(_context.Specialties, "Id", "Name", doctor.SpecialtyId);
                    return View(doctor);
                }

                _context.Add(doctor);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            }
            ViewData["Specialties"] = new SelectList(_context.Specialties, "Id", "Name", doctor.SpecialtyId);
            return View(doctor);
        }

        // GET: Doctors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }
            ViewData["Specialties"] = new SelectList(_context.Specialties, "Id", "Name", doctor.SpecialtyId);
            return View(doctor);
        }

        // POST: Doctors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Phone,Email,SpecialtyId")] Doctor doctor)
        {
            if (id != doctor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var oldDoctor = await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);

                    // If email changed, update IdentityUser
                    if (oldDoctor != null && oldDoctor.Email != doctor.Email && oldDoctor.Email != null && doctor.Email != null)
                    {
                        var (updateSucceeded, errorMsg) = await _userService.UpdateEmailAsync(oldDoctor.Email, doctor.Email);
                        if (!updateSucceeded)
                        {
                            ModelState.AddModelError("Email", errorMsg!);
                            ViewData["Specialties"] = new SelectList(_context.Specialties, "Id", "Name", doctor.SpecialtyId);
                            return View(doctor);
                        }
                    }

                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم تعديل بيانات الطبيب بنجاح.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Specialties"] = new SelectList(_context.Specialties, "Id", "Name", doctor.SpecialtyId);
            return View(doctor);
        }

        // POST: Doctors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors.Include(d => d.Appointments).FirstOrDefaultAsync(d => d.Id == id);
            
            if (doctor == null)
            {
                return NotFound();
            }

            // Check if there are related appointments
            if (doctor.Appointments.Any())
            {
                TempData["Error"] = "لا يمكن حذف هذا الطبيب لوجود مواعيد مرتبطة به.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                string doctorEmail = doctor.Email;

                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();

                // Delete the IdentityUser account if it exists
                if (!string.IsNullOrEmpty(doctorEmail))
                {
                    await _userService.DeleteUserByEmailAsync(doctorEmail);
                }

                TempData["Success"] = "تم حذف الطبيب وحساب الدخول الخاص به بنجاح.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "تم حذف بيانات الطبيب، ولكن حدث خطأ أثناء حذف الحساب الخاص به: " + ex.Message;
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(e => e.Id == id);
        }
    }
}
