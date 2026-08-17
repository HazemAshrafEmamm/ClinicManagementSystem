using ClinicManagementSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagementSystem.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Seed Roles
            string[] roles = { "Admin", "Receptionist", "Patient", "Doctor" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed Admin
            var adminUser = await userManager.FindByEmailAsync("admin@clinic.com");
            if (adminUser == null)
            {
                var user = new IdentityUser { UserName = "admin@clinic.com", Email = "admin@clinic.com", EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }

            // Seed Receptionist
            var receptionUser = await userManager.FindByEmailAsync("reception@clinic.com");
            if (receptionUser == null)
            {
                var user = new IdentityUser { UserName = "reception@clinic.com", Email = "reception@clinic.com", EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, "Reception123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Receptionist");
                    receptionUser = user;
                }
            }

            if (receptionUser != null && !context.Receptionists.Any(r => r.Email == "reception@clinic.com"))
            {
                context.Receptionists.Add(new Receptionist 
                { 
                    FullName = "هادي محمد", 
                    Phone = "01017735270", 
                    Email = "reception@clinic.com", 
                    UserId = receptionUser.Id 
                });
                await context.SaveChangesAsync();
            }

            // Seed Doctor Identity User
            var doctorUser = await userManager.FindByEmailAsync("ahmed@clinic.com");
            if (doctorUser == null)
            {
                var user = new IdentityUser { UserName = "ahmed@clinic.com", Email = "ahmed@clinic.com", EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, "Doctor123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Doctor");
                }
            }

            // Seed Dummy Data
            if (!context.Specialties.Any())
            {
                var sp1 = new Specialty { Name = "باطنة" };
                var sp2 = new Specialty { Name = "أسنان" };
                var sp3 = new Specialty { Name = "أطفال" };

                context.Specialties.AddRange(sp1, sp2, sp3);
                await context.SaveChangesAsync();
            }
        }
    }
}
