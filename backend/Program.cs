using System.Text;
using System.Text.Json.Serialization;
using Dotnet_test.Hubs;
using Dotnet_test.Infrastructure;
using Dotnet_test.Interfaces;
using Dotnet_test.Repository;
using Dotnet_test.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // serialize enums as strings in API responses
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // serialize Duration struct as string
        options.JsonSerializerOptions.Converters.Add(new DurationJsonConverter());
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register db context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// CORS configuration
builder.Services.AddCors(p =>
    p.AddPolicy(
        "defaultPolicy",
        builder =>
        {
            _ = builder
                .WithOrigins(
                    "http://localhost:5173", // Vite dev server
                    "https://localhost:5173", // Vite dev server HTTPS
                    "http://localhost:3000", // Alternative dev server
                    "https://localhost:3000" // Alternative dev server HTTPS
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
    )
);

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPartyRepository, PartyRepository>();
builder.Services.AddScoped<ISongRepository, SongRepository>();

// Register services
builder.Services.AddScoped<DataSeedingService>();

builder.Services.AddScoped<IPartyService, PartyService>(); 
builder.Services.AddScoped<ISongService, SongService>();   
builder.Services.AddScoped<IUserService, UserService>();  

builder.Services.AddSignalR();

// JWT Authentication
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            ),
        };
    });

var app = builder.Build();

// Seed database on startup
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dataSeedingService = scope.ServiceProvider.GetRequiredService<DataSeedingService>();
    var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "songs.csv");

    try
    {
        await dataSeedingService.SeedSongsFromCsvAsync(csvPath);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error occurred while seeding database from CSV");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors("defaultPolicy");
}

// HTTPS redirection for production only
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<PartyHub>("/partyHub");

app.MapControllers();

app.Run();
