namespace ClinicManagementSystem.Services
{
    public interface IUserService
    {
        Task<(bool Succeeded, string? UserId, string? TempPassword, IEnumerable<string> Errors)> CreateUserWithRoleAsync(string email, string role, string prefix);
        Task<(bool Succeeded, string? Error)> UpdateEmailAsync(string oldEmail, string newEmail);
        Task<bool> DeleteUserByEmailAsync(string email);
        Task<bool> DeleteUserByIdAsync(string id);
    }
}
