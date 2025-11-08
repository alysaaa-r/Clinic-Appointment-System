// Program.cs
using ClinicAppointmentSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add MVC services
builder.Services.AddControllersWithViews();

// ✅ Connection string (check your appsettings.json for DefaultConnection)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Example in appsettings.json:
// "DefaultConnection": "server=localhost;port=3306;database=clinic_db;user=root;password=YourPassword;TreatTinyAsBoolean=true"

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)))
           .EnableSensitiveDataLogging(false)
           .EnableDetailedErrors());

// ✅ Add session support (required if using HttpContext.Session anywhere)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Configure authentication using cookie scheme
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Account/Login";               // Redirect here if not logged in
        options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect here if unauthorized
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);  // Session duration
        options.SlidingExpiration = true;                   // Extend session while active
    });

// ✅ Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MainAdminOnly", policy =>
        policy.RequireClaim("IsMainAdmin", "true"));
    options.AddPolicy("ApprovedOnly", policy =>
        policy.RequireClaim("IsApproved", "true"));
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy =>
        policy.RequireRole("User"));
});

var app = builder.Build();

// ✅ Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Session must come before Authentication
app.UseSession();

// ✅ Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// ✅ Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
