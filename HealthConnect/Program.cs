using HealthConnect;
using HealthConnect.Data;
using HealthConnect.Hubs;
using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddRazorPages();

builder.Services.AddSignalR();

builder.Services.AddDbContext<HealthConnectContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("HealthConnect")));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
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

app.MapHub<ChatHub>("/chatHub");
app.MapControllers();

app.MapRazorPages();

app.Run();
