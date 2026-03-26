using FreelancerManagementSystem.Data;
using FreelancerManagementSystem.Interfaces;
using FreelancerManagementSystem.Services;
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
    // This generates the JSON document
    app.MapOpenApi();

    // This provides the beautiful UI at /scalar/v1
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Freelancer System Admin")
               .WithTheme(ScalarTheme.Moon);
    });

    app.UseExceptionHandler("/Home/Error");
}
else
{
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
    pattern: "{controller=Home}/{action=Index}/{id?}");

// --- 3. DATABASE INITIALIZATION ---
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();