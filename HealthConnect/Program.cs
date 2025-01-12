using HealthConnect;
using HealthConnect.Data;
using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // For DbContext, UseSqlServer, and EntityTypeBuilder


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();



// Configure DbContext
builder.Services.AddDbContext<HealthConnectContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("HealthConnect")));


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(2); 
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IPasswordHasher<Doctor_approvel>, PasswordHasher<Doctor_approvel>>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseSession();

app.UseAuthTokenMiddleware();

app.UseMiddleware<AuthTokenMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
