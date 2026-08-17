using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using ClinicManagementSystem.Services;

namespace ClinicManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReceptionistsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public ReceptionistsController(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        // GET: Receptionists
        public async Task<IActionResult> Index(string searchString)
        {
            var receptionists = _context.Receptionists.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                receptionists = receptionists.Where(r => r.FullName.Contains(searchString) || r.Email.Contains(searchString));
            }

            ViewData["CurrentSearch"] = searchString;

            return View(await receptionists.ToListAsync());
        }

        // GET: Receptionists/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Receptionists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,Phone,Email")] Receptionist receptionist)
        {
            if (ModelState.IsValid)
            {
                // Create Identity User for the receptionist using UserService
                var (succeeded, userId, tempPassword, errors) = await _userService.CreateUserWithRoleAsync(receptionist.Email, "Receptionist", "Rec");
                
                if (succeeded)
                {
                    receptionist.UserId = userId;
                }
                else
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                    return View(receptionist);
                }

                _context.Add(receptionist);
                await _context.SaveChangesAsync();
                
                if (tempPassword != null)
                {
                    TempData["Success"] = $"تم إضافة موظف الاستقبال بنجاح. كلمة المرور المؤقتة للحساب هي: {tempPassword}";
                }
                else
                {
                    TempData["Success"] = "تم إضافة موظف الاستقبال بنجاح (وتم ربطه بحساب موجود مسبقاً).";
                }
                
                return RedirectToAction(nameof(Index));
            }
            return View(receptionist);
        }

        // GET: Receptionists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var receptionist = await _context.Receptionists.FindAsync(id);
            if (receptionist == null)
            {
                return NotFound();
            }
            return View(receptionist);
        }

        // POST: Receptionists/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Phone,Email,UserId")] Receptionist receptionist)
        {
            if (id != receptionist.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var oldReceptionist = await _context.Receptionists.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
                    
                    // If email changed, update IdentityUser
                    if (oldReceptionist != null && oldReceptionist.Email != receptionist.Email && !string.IsNullOrEmpty(receptionist.UserId))
                    {
                        var (updateSucceeded, errorMsg) = await _userService.UpdateEmailAsync(oldReceptionist.Email, receptionist.Email);
                        if (!updateSucceeded)
                        {
                            ModelState.AddModelError("Email", errorMsg!);
                            return View(receptionist);
                        }
                    }

                    _context.Update(receptionist);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم تعديل بيانات موظف الاستقبال بنجاح.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReceptionistExists(receptionist.Id))
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
            return View(receptionist);
        }

        // POST: Receptionists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var receptionist = await _context.Receptionists.FindAsync(id);
            
            if (receptionist == null)
            {
                return NotFound();
            }

            try
            {
                // To avoid Foreign Key Constraint errors, we delete the Receptionist record first,
                // then delete the IdentityUser afterwards.
                string userId = receptionist.UserId;

                _context.Receptionists.Remove(receptionist);
                await _context.SaveChangesAsync();

                // Now delete the IdentityUser account if it exists
                if (!string.IsNullOrEmpty(userId))
                {
                    await _userService.DeleteUserByIdAsync(userId);
                }

                TempData["Success"] = "تم حذف موظف الاستقبال وحساب الدخول الخاص به بنجاح.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "تم حذف بيانات الموظف، ولكن حدث خطأ أثناء حذف الحساب الخاص به: " + ex.Message;
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool ReceptionistExists(int id)
        {
            return _context.Receptionists.Any(e => e.Id == id);
        }
    }
}
