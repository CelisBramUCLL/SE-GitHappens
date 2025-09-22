using Backend.Data;
using Backend.Models;
using Backend.Util;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Variables
var allowedOrigins =
    builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
var apiVersion = builder.Configuration["ApiVersion"] ?? "v1";
var basePath = $"/api/{apiVersion}";

// Database Connection
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=UserManagementDB;Username=postgres;Password=your_password_here";

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // Allow any origin for debugging
                .AllowCredentials();
        }
    );
});

var app = builder.Build();

// Optional database seeding in development or when explicitly requested
if (
    app.Environment.IsDevelopment()
    || Environment.GetEnvironmentVariable("SEED_DATABASE") == "true"
)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Apply any pending migrations
    await context.Database.MigrateAsync();

    // Seed database if needed
    await DatabaseSeeder.SeedDatabase(context);

    // If seeding was explicitly requested, exit after seeding
    if (Environment.GetEnvironmentVariable("SEED_DATABASE") == "true")
    {
        Console.WriteLine("Database seeding completed. Exiting...");
        Environment.Exit(0);
    }
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

var baseGroup = app.MapGroup(basePath);
baseGroup.MapUserEndpoints(basePath);

app.Run();
