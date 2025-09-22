using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes, string basePath)
    {
        var group = routes.MapGroup("/users");

        // Get all users
        group.MapGet(
            "/",
            async (AppDbContext db) =>
            {
                return await db.Users.ToListAsync();
            }
        );

        // Create a new user
        group.MapPost(
            "/",
            async (UserCreateRequest request, AppDbContext db) =>
            {
                var user = new User(
                    Guid.NewGuid().ToString(),
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.PasswordHash, // In production, hash the password here
                    request.BirthDate,
                    request.Username,
                    request.Role
                );

                db.Users.Add(user);
                await db.SaveChangesAsync();

                return Results.Created($"{basePath}/users/{user.Id}", user);
            }
        );

        // Get user by ID
        group.MapGet(
            "/{id}",
            async (string id, AppDbContext db) =>
            {
                var user = await db.Users.FindAsync(id);
                return user is not null ? Results.Ok(user) : Results.NotFound();
            }
        );

        // Delete user by ID
        group.MapDelete(
            "/{id}",
            async (string id, AppDbContext db) =>
            {
                var user = await db.Users.FindAsync(id);
                if (user is null)
                {
                    return Results.NotFound();
                }

                db.Users.Remove(user);
                await db.SaveChangesAsync();

                return Results.NoContent(); // 204 No Content - successful deletion
            }
        );
    }
}

// DTO for creating users
public record UserCreateRequest(
    string FirstName,
    string LastName,
    string Email,
    string PasswordHash,
    DateTime BirthDate,
    string Username,
    string Role
);
