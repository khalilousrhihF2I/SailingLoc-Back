using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration config)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var rm = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var adminPass = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
                 ?? config["Seed:AdminPassword"]
                 ?? throw new Exception("ADMIN_PASSWORD missing!");

        var renterPass = Environment.GetEnvironmentVariable("RENTER_PASSWORD")
                         ?? config["Seed:RenterPassword"]
                         ?? throw new Exception("RENTER_PASSWORD missing!");

        var ownerPass = Environment.GetEnvironmentVariable("OWNER_PASSWORD")
                         ?? config["Seed:OwnerPassword"]
                         ?? throw new Exception("OWNER_PASSWORD missing!");
        await db.Database.MigrateAsync();

        // --- Roles ---
        var roles = new[] { "Admin", "Renter", "Owner" };
        foreach (var role in roles)
        {
            if (!await rm.RoleExistsAsync(role))
                await rm.CreateAsync(new AppRole { Name = role });
        }
        // --- SailingLoc ---
        if (await um.FindByEmailAsync("voisinad7373@gmail.com") is null)
        {
            var admin = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = "voisinad7373@gmail.com",
                UserName = "voisinad7373",
                FirstName = "Ousrhih",
                LastName = "Khalil",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                UserType = "admin",
                EmailConfirmed = true
            };
            await um.CreateAsync(admin, adminPass);
            await um.AddToRoleAsync(admin, "Admin");
        }

        // --- Admin ---
        if (await um.FindByEmailAsync("admin@local.test") is null)
        {
            var admin = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = "admin@local.test",
                UserName = "admin@local.test",
                FirstName = "Admin",
                LastName = "User",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                UserType = "admin",
                EmailConfirmed = true
            };
            await um.CreateAsync(admin, adminPass);
            await um.AddToRoleAsync(admin, "Admin");
        }

        // --- Client ---
        if (await um.FindByEmailAsync("Renter@local.test") is null)
        {
            var client = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = "Renter@local.test",
                UserName = "Renter@local.test",
                FirstName = "Renter",
                LastName = "User",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                UserType = "renter",
                EmailConfirmed = true
            };
            await um.CreateAsync(client, renterPass);
            await um.AddToRoleAsync(client, "Renter");
        }

        // --- Partner ---
        if (await um.FindByEmailAsync("Owner@local.test") is null)
        {
            var partner = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = "Owner@local.test",
                UserName = "Owner@local.test",
                FirstName = "Owner",
                LastName = "User",
                BirthDate = DateTime.UtcNow.AddYears(-35),
                UserType = "owner",
                EmailConfirmed = true
            };
            await um.CreateAsync(partner, ownerPass);
            await um.AddToRoleAsync(partner, "Owner");
        }
    }
}
