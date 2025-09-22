using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Util;

public static class DatabaseSeeder
{
    public static async Task SeedDatabase(AppDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Check if users already exist
        if (await context.Users.AnyAsync())
        {
            Console.WriteLine("Database already contains users. Skipping seed.");
            return;
        }

        Console.WriteLine("Seeding database with test users...");

        // Create test users with hashed passwords (for demo purposes using simple hash)
        var testUsers = new List<User>
        {
            new User(
                id: Guid.NewGuid().ToString(),
                firstName: "John",
                lastName: "Doe",
                email: "john.doe@example.com",
                passwordHash: BCrypt.Net.BCrypt.HashPassword("password123"), // Hashed password
                birthDate: DateTime.SpecifyKind(new DateTime(1990, 5, 15), DateTimeKind.Utc),
                username: "johndoe",
                role: "User"
            ),
            new User(
                id: Guid.NewGuid().ToString(),
                firstName: "Jane",
                lastName: "Smith",
                email: "jane.smith@example.com",
                passwordHash: BCrypt.Net.BCrypt.HashPassword("password123"),
                birthDate: DateTime.SpecifyKind(new DateTime(1988, 8, 22), DateTimeKind.Utc),
                username: "janesmith",
                role: "Admin"
            ),
            new User(
                id: Guid.NewGuid().ToString(),
                firstName: "Bob",
                lastName: "Johnson",
                email: "bob.johnson@example.com",
                passwordHash: BCrypt.Net.BCrypt.HashPassword("password123"),
                birthDate: DateTime.SpecifyKind(new DateTime(1995, 12, 3), DateTimeKind.Utc),
                username: "bobjohnson",
                role: "User"
            ),
            new User(
                id: Guid.NewGuid().ToString(),
                firstName: "Alice",
                lastName: "Williams",
                email: "alice.williams@example.com",
                passwordHash: BCrypt.Net.BCrypt.HashPassword("password123"),
                birthDate: DateTime.SpecifyKind(new DateTime(1992, 3, 18), DateTimeKind.Utc),
                username: "alicewilliams",
                role: "Moderator"
            ),
            new User(
                id: Guid.NewGuid().ToString(),
                firstName: "Charlie",
                lastName: "Brown",
                email: "charlie.brown@example.com",
                passwordHash: BCrypt.Net.BCrypt.HashPassword("password123"),
                birthDate: DateTime.SpecifyKind(new DateTime(1987, 11, 7), DateTimeKind.Utc),
                username: "charliebrown",
                role: "User"
            ),
        };

        // Add users to context
        await context.Users.AddRangeAsync(testUsers);
        await context.SaveChangesAsync();

        Console.WriteLine($"Successfully seeded {testUsers.Count} test users:");
        foreach (var user in testUsers)
        {
            Console.WriteLine(
                $"- {user.FirstName} {user.LastName} ({user.Username}) - {user.Role}"
            );
        }
    }
}
