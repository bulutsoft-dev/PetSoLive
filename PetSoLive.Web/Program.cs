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

// Add services to the container.
builder.Services.AddControllersWithViews();

// SMTP Settings (static values)
var smtpSettings = new SmtpSettings
{
    Host = "smtp.gmail.com",  // SMTP_HOST
    Port = 587,               // SMTP_PORT
    Username = "petsolivesoft@gmail.com",  // SMTP_USERNAME
    Password = "lrgl khdo bnhf rqhj", // SMTP_PASSWORD
    FromEmail = "petsolivesoft@gmail.com",  // SMTP_FROM_EMAIL
    EnableSsl = true          // SMTP_ENABLE_SSL
};

builder.Services.AddSingleton(smtpSettings);

// SQLite Database Connection String (static value)
var connectionString = "Data Source=app.db"; // DATABASE_CONNECTION_STRING
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

// Add distributed memory cache and session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container
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

// Authentication and Authorization
builder.Services.AddAuthentication("Cookies")
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Alt dizini ayarla (PathBase)
app.UsePathBase("/petsolive");
app.Use((context, next) =>
{
    context.Request.PathBase = "/petsolive";
    return next();
});

// Configure the HTTP request pipeline.
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
