using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FauluApartmentAPI.Data;
using FauluApartmentAPI.Data.Entities;
using FauluApartmentAPI.Data.Repositories;
using FauluApartmentAPI.Services;
using FauluApartmentAPI.Middleware;
using FluentValidation;
using FauluApartmentAPI.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add database context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=FauluApartmentDb;Trusted_Connection=true;";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

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
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
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

// Only redirect to HTTPS in production — in development this strips
// the Authorization header on redirect and causes 401s
if (!app.Environment.IsDevelopment())
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

// NOTE: The old "Register TransactionStatus URLs" startup block was removed here.
// Daraja has no /mpesa/transactionstatus/v1/registerurl endpoint — it was always
// returning 404.001.01 "Resource not found" on every startup. TransactionStatus
// doesn't use a pre-registered URL at all; ResultURL/QueueTimeOutURL are passed
// inline on each query in DarajaService.RequestTransactionStatusAsync, which is
// unaffected by this removal.

app.Run();