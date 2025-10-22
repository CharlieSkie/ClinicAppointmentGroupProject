using ClinicAppointmentGroupProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicAppointmentGroupProject
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var context = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();

                // Ensure database is created
                await context.Database.EnsureCreatedAsync();

                string[] roleNames = { "Admin", "Doctor", "Client" };

                // Create roles
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                        if (result.Succeeded)
                        {
                            Console.WriteLine($"Created role: {roleName}");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to create role {roleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Role already exists: {roleName}");
                    }
                }

                // Create default admin user
                var adminUser = await userManager.FindByEmailAsync("admin@clinic.com");
                if (adminUser == null)
                {
                    Console.WriteLine("Creating admin user...");
                    adminUser = new ApplicationUser
                    {
                        UserName = "admin@clinic.com",
                        Email = "admin@clinic.com",
                        FullName = "System Administrator",
                        UserType = UserType.Admin,
                        EmailConfirmed = true,
                        ApprovalStatus = ApprovalStatus.Approved, // CRITICAL: This was missing
                        ApprovedAt = DateTime.Now,
                        ApprovedBy = "System"
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin123!");
                    if (result.Succeeded)
                    {
                        Console.WriteLine("Admin user created successfully.");

                        // Add to Admin role
                        var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                        if (roleResult.Succeeded)
                        {
                            Console.WriteLine("Admin user added to Admin role.");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to add admin to role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    Console.WriteLine("Admin user already exists.");

                    // Ensure admin is approved
                    if (adminUser.ApprovalStatus != ApprovalStatus.Approved)
                    {
                        adminUser.ApprovalStatus = ApprovalStatus.Approved;
                        adminUser.ApprovedAt = DateTime.Now;
                        adminUser.ApprovedBy = "System";
                        await userManager.UpdateAsync(adminUser);
                        Console.WriteLine("Admin user approval status updated to Approved.");
                    }

                    // Ensure admin has Admin role
                    var isInRole = await userManager.IsInRoleAsync(adminUser, "Admin");
                    if (!isInRole)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        Console.WriteLine("Admin user added to Admin role.");
                    }
                }
            }
        }
    }
}