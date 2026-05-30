using DFIR.CaseManagement.Entities;
using DFIR.CaseManagement.Services;
using Microsoft.EntityFrameworkCore;

namespace DFIR.CaseManagement.Data;

/// <summary>Applies pending migrations and seeds the three default users on first run.</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync())
        {
            logger.LogInformation("Seed skipped: users already present.");
            return;
        }

        var users = new User[]
        {
            Build(new Admin(), "admin", "admin@dfir.local", "System Administrator", "Admin123!"),
            Build(new Analyst(), "analyst", "analyst@dfir.local", "Forensic Analyst", "Analyst123!"),
            Build(new Viewer(), "viewer", "viewer@dfir.local", "Read-Only Viewer", "Viewer123!")
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} default users (admin / analyst / viewer).", users.Length);
    }

    private static User Build(User user, string username, string email, string fullName, string password)
    {
        user.Username = username;
        user.Email = email;
        user.FullName = fullName;
        user.IsActive = true;
        user.SetPasswordHash(PasswordHasher.Hash(password));
        return user;
    }
}
