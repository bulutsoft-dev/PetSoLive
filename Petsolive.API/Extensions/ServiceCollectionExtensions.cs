using Microsoft.Extensions.DependencyInjection;
using PetSoLive.Core.Interfaces;
using PetSoLive.Business.Services;
using PetSoLive.Data.Repositories;
using PetSoLive.Data;
using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Services;
using PetSoLive.Infrastructure.Repositories;

namespace Petsolive.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPetSoLiveDependencies(this IServiceCollection services, string connectionString)
    {
        // DbContext (PostgreSQL)
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repository katmanı
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRepository<User>, UserRepository>();
        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<IPetOwnerRepository, PetOwnerRepository>();
        services.AddScoped<IVeterinarianRepository, VeterinarianRepository>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IAdoptionRepository, AdoptionRepository>();
        services.AddScoped<IAdoptionRequestRepository, AdoptionRequestRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IHelpRequestRepository, HelpRequestRepository>();
        services.AddScoped<ILostPetAdRepository, LostPetAdRepository>();

        // Service katmanı
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPetService, PetService>();
        services.AddScoped<IPetOwnerService, PetOwnerService>();
        services.AddScoped<IVeterinarianService, VeterinarianService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IAdoptionService, AdoptionService>();
        services.AddScoped<IAdoptionRequestService, AdoptionRequestService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IHelpRequestService, HelpRequestService>();
        services.AddScoped<ILostPetAdService, LostPetAdService>();
        
        // Register SmtpClient for DI
        services.AddTransient<System.Net.Mail.SmtpClient>();
        // Register ISmtpClient for DI
        services.AddScoped<ISmtpClient, SmtpClientWrapper>();
        
        services.AddScoped<IEmailService, EmailService>();

        // ServiceManager
        services.AddScoped<IServiceManager, ServiceManager>();

        // AutoMapper
        services.AddAutoMapper(typeof(Petsolive.API.Mapping.MappingProfile));

        // SMTP Settings registration
        var smtpSettings = new PetSoLive.Core.Entities.SmtpSettings
        {
            Host = Environment.GetEnvironmentVariable("SMTP_HOST")!,
            Port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT")!),
            Username = Environment.GetEnvironmentVariable("SMTP_USERNAME")!,
            Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD")!,
            FromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL")!,
            EnableSsl = bool.TryParse(Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL"), out var enableSsl) && enableSsl
        };
        services.AddSingleton(smtpSettings);

        return services;
    }
}