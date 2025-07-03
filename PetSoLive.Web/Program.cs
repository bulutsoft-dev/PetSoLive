using System.Globalization;
using DotNetEnv;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using PetSoLive.Business.Services;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Services;
using PetSoLive.Data;
using PetSoLive.Data.Repositories;
using PetSoLive.Infrastructure.Repositories;
using SmtpSettings = PetSoLive.Core.Entities.SmtpSettings;

// .env dosyasını builder'dan önce yükle
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// SMTP environment variable'larını logla
Console.WriteLine($"SMTP_HOST: {Environment.GetEnvironmentVariable("SMTP_HOST")}");
Console.WriteLine($"SMTP_PORT: {Environment.GetEnvironmentVariable("SMTP_PORT")}");
Console.WriteLine($"SMTP_USERNAME: {Environment.GetEnvironmentVariable("SMTP_USERNAME")}");
Console.WriteLine($"SMTP_PASSWORD: {Environment.GetEnvironmentVariable("SMTP_PASSWORD")}");
Console.WriteLine($"SMTP_FROM_EMAIL: {Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL")}");
Console.WriteLine($"SMTP_ENABLE_SSL: {Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL")}");

// Add services to the container.
builder.Services.AddControllersWithViews();

// SMTP Settings
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

// SmtpClient'ı ayarlarla birlikte DI'ya ekle
builder.Services.AddTransient<System.Net.Mail.SmtpClient>(sp =>
{
    var smtp = sp.GetRequiredService<SmtpSettings>();
    var client = new System.Net.Mail.SmtpClient
    {
        Host = smtp.Host,
        Port = smtp.Port,
        EnableSsl = smtp.EnableSsl,
        Credentials = new System.Net.NetworkCredential(smtp.Username, smtp.Password)
    };
    return client;
});

// PostgreSQL Database Connection String
var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString); // PostgreSQL için Npgsql kullanılıyor
});
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Add distributed memory cache and session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container
builder.Services.AddScoped<ISmtpClient, SmtpClientWrapper>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRepository<User>, UserRepository>();
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
builder.Services.AddScoped<IVeterinarianService, VeterinarianService>();
builder.Services.AddScoped<IVeterinarianRepository, VeterinarianRepository>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IServiceManager, ServiceManager>();

// Authentication and Authorization
builder.Services.AddAuthentication("Cookies")
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// Localization servislerini ekle
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// MVC için localization'ı aktif et
builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


// Desteklenen dilleri tanımla
var supportedCultures = new[] { "en-US", "tr-TR" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[1])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

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
