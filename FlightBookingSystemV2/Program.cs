using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FlightBookingSystem.Configuration;
using FlightBookingSystem.Data;
using FlightBookingSystem.Middleware;
using FlightBookingSystem.Repositories.Interfaces;
using FlightBookingSystem.Repositories.Implementations;
using FlightBookingSystem.Services.Interfaces;
using FlightBookingSystem.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════
// 1. DATABASE — Entity Framework Core + SQL Server
// ═══════════════════════════════════════════════════════
builder.Services.AddDbContext<FlightDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ═══════════════════════════════════════════════════════
// 2. JWT SETTINGS
// ═══════════════════════════════════════════════════════
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings are missing from appsettings.json");
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

// ═══════════════════════════════════════════════════════
// 3. JWT AUTHENTICATION
// ═══════════════════════════════════════════════════════
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSettings.Issuer,
            ValidAudience            = jwtSettings.Audience,
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

builder.Services.AddAuthorization();

// ═══════════════════════════════════════════════════════
// 4. REPOSITORIES  (one interface → one implementation)
// ═══════════════════════════════════════════════════════
builder.Services.AddScoped<IAuthRepository,        AuthRepository>();
builder.Services.AddScoped<IUserRepository,        UserRepository>();
builder.Services.AddScoped<IFlightRepository,      FlightRepository>();
builder.Services.AddScoped<IBookingRepository,     BookingRepository>();
builder.Services.AddScoped<ICheckInRepository,     CheckInRepository>();
builder.Services.AddScoped<IAdminFlightRepository, AdminFlightRepository>();
builder.Services.AddScoped<IFareRepository,        FareRepository>();
builder.Services.AddScoped<IDeleteRepository,      DeleteRepository>();

// ═══════════════════════════════════════════════════════
// 5. SERVICES  (one interface → one implementation)
// ═══════════════════════════════════════════════════════
builder.Services.AddScoped<IAuthService,    AuthService>();
builder.Services.AddScoped<IAdminService,   AdminService>();
builder.Services.AddScoped<IFlightService,  FlightService>();
builder.Services.AddScoped<IFareService,    FareService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<IEmailService,   EmailService>();

// ═══════════════════════════════════════════════════════
// 6. CONTROLLERS + CORS
// ═══════════════════════════════════════════════════════
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ═══════════════════════════════════════════════════════
// 7. SWAGGER — with JWT Bearer support
// ═══════════════════════════════════════════════════════
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Flight Booking System API",
        Version     = "v1",
        Description = "REST API for Flight Search, Booking, Check-in and Admin Operations"
    });

    // Allow JWT token entry in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.ApiKey,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter: Bearer {your token here}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ═══════════════════════════════════════════════════════
// BUILD THE APP
// ═══════════════════════════════════════════════════════
var app = builder.Build();

// ═══════════════════════════════════════════════════════
// 8. INITIALIZE DATABASE on startup
// ═══════════════════════════════════════════════════════
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FlightDbContext>();
    // Use Migrate() instead of EnsureCreated() — supports Add-Migration & Update-Database
    // First time: creates DB + all tables. Future: applies any new migrations.
    db.Database.EnsureCreated();
    if (!db.Admins.Any())
    {
        db.Admins.Add(new FlightBookingSystem.Models.Entities.Admin
        {
            Username = "admin",
            Email    = "admin@flightbooking.com",
            Password = BCrypt.Net.BCrypt.HashPassword("admin123")
        });
        db.SaveChanges();
    }
}

// ═══════════════════════════════════════════════════════
// 9. MIDDLEWARE PIPELINE  (order is critical)
// ═══════════════════════════════════════════════════════
// Step 1 — Global exception handler wraps everything
app.UseGlobalExceptionHandler();

// Step 2 — Swagger UI (available in all environments)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Flight Booking System v1");
    c.RoutePrefix = string.Empty; // Swagger loads at http://localhost:5000
});

// Step 3 — CORS
app.UseCors("AllowAll");

// Step 4 — HTTPS redirect
app.UseHttpsRedirection();

// Step 5 — Authentication (WHO are you? — reads the JWT)
app.UseAuthentication();

// Step 6 — Authorization (WHAT can you do? — checks roles)
app.UseAuthorization();

// Step 7 — Route to controllers
app.MapControllers();

app.Run();
