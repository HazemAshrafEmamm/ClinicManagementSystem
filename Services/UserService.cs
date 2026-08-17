using Microsoft.AspNetCore.Identity;

namespace ClinicManagementSystem.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UserService(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<(bool Succeeded, string? UserId, string? TempPassword, IEnumerable<string> Errors)> CreateUserWithRoleAsync(string email, string role, string prefix)
        {
            var user = await _userManager.FindByEmailAsync(email);
            
            if (user != null)
            {
                bool isInRole = await _userManager.IsInRoleAsync(user, role);
                return (isInRole, user.Id, null, isInRole ? Array.Empty<string>() : new[] { "هذا البريد مستخدم لدور آخر في النظام." });
            }

            string tempPassword = prefix + Guid.NewGuid().ToString("N").Substring(0, 6) + "A!1";
            user = new IdentityUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, tempPassword);
            
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                return (true, user.Id, tempPassword, Array.Empty<string>());
            }
            
            return (false, null, null, result.Errors.Select(e => e.Description));
        }

        public async Task<(bool Succeeded, string? Error)> UpdateEmailAsync(string oldEmail, string newEmail)
        {
            var user = await _userManager.FindByEmailAsync(oldEmail);
            if (user != null)
            {
                var existingUser = await _userManager.FindByEmailAsync(newEmail);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    return (false, "البريد الإلكتروني الجديد مستخدم بالفعل لحساب آخر.");
                }

                user.Email = newEmail;
                user.UserName = newEmail;
                user.NormalizedEmail = newEmail.ToUpper();
                user.NormalizedUserName = newEmail.ToUpper();
                await _userManager.UpdateAsync(user);
            }
            return (true, null);
        }

        public async Task<bool> DeleteUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) return true;
            
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, roles);
                }
                var result = await _userManager.DeleteAsync(user);
                return result.Succeeded;
            }
            return true;
        }

        public async Task<bool> DeleteUserByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return true;
            
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, roles);
                }
                var result = await _userManager.DeleteAsync(user);
                return result.Succeeded;
            }
            return true;
        }
    }
}
