using Had360back.Models;
using Microsoft.AspNetCore.Identity;

namespace Had360back.Data.Seeders;

public static class DataSeeder
{
    public static async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

        string[] roleNames = { "Admin", "ContentManager", "Organization", "User" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new Role { Name = roleName });
            }
        }

        // Seed 2 Default Admins
        var admin1Email = "admin1@had360.com";
        if (await userManager.FindByEmailAsync(admin1Email) == null)
        {
            var admin1 = new User
            {
                UserName = "AdminOne",
                Email = admin1Email,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(admin1, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin1, "Admin");
            }
        }

        var admin2Email = "admin2@had360.com";
        if (await userManager.FindByEmailAsync(admin2Email) == null)
        {
            var admin2 = new User
            {
                UserName = "AdminTwo",
                Email = admin2Email,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(admin2, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin2, "Admin");
            }
        }
    }
}
