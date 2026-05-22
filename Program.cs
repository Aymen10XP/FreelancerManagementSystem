using FreelancerManagementSystem.Data;
using FreelancerManagementSystem.Interfaces;
using FreelancerManagementSystem.Services;
using FreelancerManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore; // For the new Pro UI

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVICES CONFIGURATION ---
builder.Services.AddControllers();

// Native .NET 10 OpenAPI Generator
builder.Services.AddOpenApi(options =>
{
    // This adds the "Authorize" button to the new UI
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Freelancer Management API";
        document.Info.Version = "v1";
        return Task.CompletedTask;
    });
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ASP.NET Core Identity Configuration
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    // Password requirements
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Email sender
builder.Services.AddTransient<FreelancerManagementSystem.Services.Email.IEmailSender, FreelancerManagementSystem.Services.Email.MailKitEmailSender>();

// Configure cookie authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
});

// JSON Handling (Prevents infinite loops in your models)
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

builder.Services.AddScoped<IAuthService, AuthService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// --- 2. MIDDLEWARE PIPELINE ---
if (app.Environment.IsDevelopment())
{
    // In Development show the detailed exception page so developers can see the stack trace
    app.UseDeveloperExceptionPage();

    // This generates the JSON document
    app.MapOpenApi();

    // This provides the beautiful UI at /scalar/v1
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Freelancer System Admin")
               .WithTheme(ScalarTheme.Moon);
    });
}
else
{
    // In Production use a generic error handler and enable HSTS
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");

// Security order is vital: Authentication FIRST, then Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// --- 3. DATABASE INITIALIZATION ---
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
