using CivicAlert.Data;
using CivicAlert.Entities;
using CivicAlert.Middleware;
using CivicAlert.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Database (PostgreSQL via Entity Framework Core)
// ==========================================
var postgresConn = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? "Host=localhost;Port=5432;Database=civicalert;Username=postgres;Password=postgres";

bool isPostgresAvailable = false;
try
{
    using var testConn = new Npgsql.NpgsqlConnection(postgresConn);
    testConn.Open();
    isPostgresAvailable = true;
}
catch
{
    Console.WriteLine("[Database Notice] PostgreSQL is not reachable at the configured host. Using InMemory database for local development/preview.");
}

builder.Services.AddDbContext<CivicAlertDbContext>(opt =>
{
    if (isPostgresAvailable) opt.UseNpgsql(postgresConn);
    else opt.UseInMemoryDatabase("CivicAlertDb");
});

// ==========================================
// 2. Application Services
// ==========================================
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReportService, ReportService>();

// ==========================================
// 3. Controllers & JSON Options
// ==========================================
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// ==========================================
// 4. Authentication & Authorization (JWT)
// ==========================================
var jwtKey = builder.Configuration["Jwt:Key"] ?? "CivicAlert_Super_Secure_Secret_Key_For_Jwt_HMAC_SHA256_Auth_Token_2026_DotNet10";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CivicAlertApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CivicAlertClients";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.RequireHttpsMetadata = false;
        opt.SaveToken = true;
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("ClientPolicy", p => p.RequireRole("ROLE_CLIENT", "CLIENT"));
    opt.AddPolicy("AdminPolicy", p => p.RequireRole("ROLE_ADMIN", "ADMIN"));
});

// ==========================================
// 5. CORS & Swagger / OpenAPI Documentation
// ==========================================
builder.Services.AddCors(opt => opt.AddDefaultPolicy(p => p.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CivicAlert Web API - Standard C# ASP.NET Core 10 (PostgreSQL)",
        Version = "v1",
        Description = "Standard, idiomatic C# ASP.NET Core Web API with Controllers, Services, DTOs, Entities, and PostgreSQL (Npgsql) persistence."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ==========================================
// 6. Global Exception Handling Middleware
// ==========================================
app.UseGlobalExceptionHandler();

// ==========================================
// 7. Database Initialization & Seeding
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CivicAlertDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        var citizen = new AppUser
        {
            FirstName = "Jane",
            LastName = "Citizen",
            Email = "citizen@civicalert.gov",
            Password = BCrypt.Net.BCrypt.HashPassword("Password123!", 12),
            Roles = [Role.CLIENT]
        };

        var admin = new AppUser
        {
            FirstName = "Marcus",
            LastName = "Vance",
            Email = "admin@civicalert.gov",
            Password = BCrypt.Net.BCrypt.HashPassword("Admin123!", 12),
            Roles = [Role.ADMIN, Role.CLIENT]
        };

        db.Users.AddRange(citizen, admin);
        db.SaveChanges();

        db.Reports.AddRange(
            new ReportModel
            {
                Title = "Hazardous Pothole on 5th Avenue",
                Description = "Deep asphalt deterioration causing vehicular tire damage and hazard for cyclists.",
                Category = "Road Hazards",
                Latitude = 40.7128,
                Longitude = -74.0060,
                CreatedAt = DateTime.UtcNow.AddHours(-3),
                Status = ReportStatus.OPEN,
                UserId = citizen.Id,
                User = citizen,
                ImagePaths = ["uploads/pothole_sample.jpg"]
            },
            new ReportModel
            {
                Title = "Damaged Park Light Post Near Autumn Walkway",
                Description = "Streetlight fixture leaning precariously with shattered lamp housing following heavy windstorm.",
                Category = "Street Lighting",
                Latitude = 40.7850,
                Longitude = -73.9682,
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                Status = ReportStatus.IN_PROGRESS,
                UserId = citizen.Id,
                User = citizen,
                ImagePaths = ["uploads/streetlight_sample.jpg"]
            }
        );
        db.SaveChanges();
    }
}

// ==========================================
// 8. HTTP Pipeline (CORS, Static Files, Swagger)
// ==========================================
app.UseCors();

var uploadDir = Path.Combine(builder.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadDir),
    RequestPath = "/uploads"
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CivicAlert API v1");
    c.RoutePrefix = "swagger";
    c.DisplayRequestDuration();
});

// Interactive Swagger UI at root /
app.MapMethods("/", ["GET", "HEAD"], () => Results.Content("""
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>CivicAlert Web API - Standard C# ASP.NET Core 10 (PostgreSQL)</title>
  <link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5.18.2/swagger-ui.css" />
  <link rel="preconnect" href="https://fonts.googleapis.com">
  <link rel="preconnect" href="https://fonts.gstatic.com">
  <link href="https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700&family=JetBrains+Mono:wght@400;500&display=swap" rel="stylesheet">
  <style>
    body { margin: 0; background: #f8fafc; font-family: 'Plus Jakarta Sans', sans-serif; }
    .civic-banner { background: #0f172a; color: #ffffff; padding: 16px 24px; display: flex; align-items: center; justify-content: space-between; border-bottom: 1px solid #1e293b; }
    .civic-title { font-size: 18px; font-weight: 700; letter-spacing: -0.02em; display: flex; align-items: center; gap: 10px; }
    .civic-badge { background: #1e293b; color: #38bdf8; font-family: 'JetBrains Mono', monospace; font-size: 11px; padding: 3px 8px; border-radius: 4px; border: 1px solid #334155; }
    .civic-badge.pg { color: #34d399; }
    .civic-credentials { font-size: 12px; color: #94a3b8; display: flex; gap: 16px; }
    .civic-credentials code { color: #f1f5f9; background: #1e293b; padding: 2px 6px; border-radius: 3px; font-family: 'JetBrains Mono', monospace; }
    .swagger-ui .topbar { display: none; }
    .swagger-ui { max-width: 1300px; margin: 0 auto; padding: 20px; }
  </style>
</head>
<body>
  <div class="civic-banner">
    <div class="civic-title">
      CivicAlert Web API
      <span class="civic-badge">C# ASP.NET Core 10</span>
      <span class="civic-badge pg">PostgreSQL (Npgsql)</span>
    </div>
    <div class="civic-credentials">
      <span>Demo Citizen: <code>citizen@civicalert.gov</code> / <code>Password123!</code></span>
      <span>Demo Admin: <code>admin@civicalert.gov</code> / <code>Admin123!</code></span>
    </div>
  </div>
  <div id="swagger-ui"></div>
  <script src="https://unpkg.com/swagger-ui-dist@5.18.2/swagger-ui-bundle.js"></script>
  <script src="https://unpkg.com/swagger-ui-dist@5.18.2/swagger-ui-standalone-preset.js"></script>
  <script>
    window.onload = () => {
      window.ui = SwaggerUIBundle({
        url: '/swagger/v1/swagger.json',
        dom_id: '#swagger-ui',
        deepLinking: true,
        presets: [SwaggerUIBundle.presets.apis, SwaggerUIStandalonePreset],
        layout: "BaseLayout",
        displayRequestDuration: true,
        docExpansion: 'list',
        filter: true
      });
    };
  </script>
</body>
</html>
""", "text/html"));

app.UseAuthentication();
app.UseAuthorization();

// ==========================================
// 9. Map Controllers
// ==========================================
app.MapControllers();

app.Run();
