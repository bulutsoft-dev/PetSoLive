using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using PetSoLive.Business.Services;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Services;
using PetSoLive.Data;
using PetSoLive.Data.Repositories;
using PetSoLive.Infrastructure.Repositories;
using SmtpSettings = PetSoLive.Core.Entities.SmtpSettings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

Env.Load();

var smtpSettings = new SmtpSettings
{
    Host = Environment.GetEnvironmentVariable("SMTP_HOST")!,
    Port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT")!),
    Username = Environment.GetEnvironmentVariable("SMTP_USERNAME")!,
    Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD")!,
    FromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL")!,
    EnableSsl = bool.TryParse(Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL"), out var enableSsl) && enableSsl
};

builder.Services.AddSingleton(smtpSettings);

var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("DATABASE_CONNECTION_STRING environment variable is not set.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("PetSoLive.Data"));
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRepository<User>, UserRepository>();
builder.Services.AddScoped<IRepository<Assistance>, AssistanceRepository>();
builder.Services.AddScoped<IAssistanceService, AssistanceService>();
builder.Services.AddScoped<IAdoptionService, AdoptionService>();
builder.Services.AddScoped<IAdoptionRepository, AdoptionRepository>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IAdoptionRepository, AdoptionRepository>();
builder.Services.AddScoped<IAdoptionService, AdoptionService>();
builder.Services.AddScoped<IPetOwnerRepository, PetOwnerRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPetOwnerService, PetOwnerService>();
builder.Services.AddScoped<IAdoptionRequestRepository, AdoptionRequestRepository>();
builder.Services.AddScoped<IAdoptionRequestService, AdoptionRequestService>();

builder.Services.AddScoped<ILostPetAdRepository, LostPetAdRepository>();
builder.Services.AddScoped<ILostPetAdService, LostPetAdService>();

builder.Services.AddScoped<IHelpRequestRepository, HelpRequestRepository>();
builder.Services.AddScoped<IHelpRequestService, HelpRequestService>();

builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddScoped<IVeterinarianService, VeterinarianService>();
builder.Services.AddScoped<IVeterinarianRepository, VeterinarianRepository>();

builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();

builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();

builder.Services.AddScoped<IUserRepository, UserRepository>();



builder.Services.AddAuthentication("Cookies")
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
