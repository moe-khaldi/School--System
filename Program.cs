using Microsoft.EntityFrameworkCore;
using UniversityCourseEnrollment.Data;
using UniversityCourseEnrollment.Services;

var builder = WebApplication.CreateBuilder(args);

// Console logging is reliable for local development and avoids Windows Event Log permission issues.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<GpaService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<TeacherService>();
builder.Services.AddScoped<EnrollmentService>();
builder.Services
    .AddAuthentication("UniversityCookie")
    .AddCookie("UniversityCookie", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization();
var app = builder.Build();

// Apply migrations and insert the local demo dataset on first startup.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbInitializer.SeedAsync(dbContext);
}

// Always send unhandled request errors to a safe page. Detailed exception data stays in server logs.
app.UseExceptionHandler("/Home/Error");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
