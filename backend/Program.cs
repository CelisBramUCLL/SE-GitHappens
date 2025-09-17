using Backend.Models;
var builder = WebApplication.CreateBuilder(args);

// Variables
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();
var apiVersion = builder.Configuration["ApiVersion"] ?? "v1";
var basePath = $"/api/{apiVersion}";

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<List<Todo>>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .SetIsOriginAllowed(origin => true) // Allow any origin for debugging
              .AllowCredentials();
    });
});

var app = builder.Build();

// Middleware`
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();  


var baseGroup = app.MapGroup(basePath);
baseGroup.MapTodoEndpoints(basePath);

app.Run();
