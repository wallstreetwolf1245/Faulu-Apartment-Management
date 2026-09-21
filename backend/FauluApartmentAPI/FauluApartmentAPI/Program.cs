using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.Text;
using FauluApartmentAPI.Data;
using FauluApartmentAPI.Data.Entities;
using FauluApartmentAPI.Data.Repositories;
using FauluApartmentAPI.Services;
using FauluApartmentAPI.Middleware;
using FluentValidation;
using FauluApartmentAPI.Validators;

// Allow DateTime values with Kind=Unspecified to be written to
// PostgreSQL timestamptz columns (treated as UTC). SQL Server never
// enforced DateTime.Kind, so this restores that lenient behavior
// after migrating providers. TODO: fix DateTime.Kind at the source
// (DTOs/entities) and remove this switch when time allows.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Bind Kestrel to Vercel's PORT — required for container Functions.
// Falls back to 8080 for local Docker testing.
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add database context
var connectionString = BuildConnectionString(builder.Configuration);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "your-secret-key-change-this-in-production");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Manager", policy => policy.RequireRole("Manager", "Admin"));
    options.AddPolicy("Tenant", policy => policy.RequireRole("Tenant"));
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// CORS: withCredentials on the frontend means we can't use AllowAnyOrigin().
// Credentialed cross-origin requests require the server to echo back the
// exact requesting origin (not "*") and explicitly allow credentials.
// TODO: replace the placeholder with your real Vercel production domain
// (and add any preview-deployment domains you want to allow too).
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "https://faulu-apartment-management-cy8u.vercel.app", // TODO: replace with real domain               
                 "http://localhost:5173"                          // local dev (Vite)
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateBuildingValidator>();

// Add memory cache and HTTP client (required by DarajaService)
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ILeaseRepository, LeaseRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<IMaintenanceOrderRepository, MaintenanceOrderRepository>();
builder.Services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();

// Register services
builder.Services.AddScoped<IBuildingService, BuildingService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ILeaseService, LeaseService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IDarajaService, DarajaService>();
builder.Services.AddScoped<IReversalService, ReversalService>();
builder.Services.AddScoped<ITransactionStatusLogRepository, TransactionStatusLogRepository>();

// Add logging
builder.Services.AddLogging(options =>
{
    options.AddConsole();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Skip HTTPS redirection when running as a Vercel container — Vercel
// terminates TLS at its edge and forwards plain HTTP internally, so the
// app itself never sees an HTTPS request and this would otherwise
// redirect-loop or break every request in production. Vercel sets
// the VERCEL env var automatically on container Functions.
var isVercel = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VERCEL"));
if (!app.Environment.IsDevelopment() && !isVercel)
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

    try
    {
        await context.Database.MigrateAsync();

        // Seed roles
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole<int> { Name = "Admin" });
        if (!await roleManager.RoleExistsAsync("Manager"))
            await roleManager.CreateAsync(new IdentityRole<int> { Name = "Manager" });
        if (!await roleManager.RoleExistsAsync("Tenant"))
            await roleManager.CreateAsync(new IdentityRole<int> { Name = "Tenant" });
        if (!await roleManager.RoleExistsAsync("Owner"))
            await roleManager.CreateAsync(new IdentityRole<int> { Name = "Owner" });

        // Seed default admin user
        var adminEmail = "admin@fauluapp.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                IsActive = true,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin@123");
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while seeding the database");
    }
}

// Register C2B URLs with Daraja on startup
// In dev: update ConfirmationUrl/ValidationUrl in appsettings.json with your ngrok URL first
using (var scope = app.Services.CreateScope())
{
    var darajaService = scope.ServiceProvider.GetRequiredService<IDarajaService>();
    var registered = await darajaService.RegisterC2BUrlsAsync();
    if (!registered)
        app.Logger.LogWarning("Daraja C2B URL registration failed — check credentials and ngrok URL in appsettings.json");
    else
        app.Logger.LogInformation("Daraja C2B URLs registered successfully");
}

app.Run();

static string BuildConnectionString(IConfiguration configuration)
{
    // Render (and most PaaS providers) inject a single DATABASE_URL
    // in postgres://user:pass@host:port/db format. Npgsql needs
    // key-value format, so parse it when present.
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : "";
        var database = uri.AbsolutePath.TrimStart('/');
        var port = uri.Port > 0 ? uri.Port : 5432;

        return $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
    }

    // Local dev fallback: appsettings.Development.json's DefaultConnection,
    // or the hardcoded default if that's missing too.
    return configuration.GetConnectionString("DefaultConnection")
        ?? "Host=localhost;Database=FauluApartmentDb;Username=postgres;Password=postgres;";
}