using Microsoft.Extensions.DependencyInjection;
using PetSoLive.Core.Interfaces;
using PetSoLive.Business.Services;
using PetSoLive.Data.Repositories;
using PetSoLive.Data;
using Microsoft.EntityFrameworkCore;
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
        services.AddScoped<IEmailService, EmailService>();

        // ServiceManager
        services.AddScoped<IServiceManager, ServiceManager>();

        // AutoMapper
        services.AddAutoMapper(typeof(Petsolive.API.Mapping.MappingProfile));

        return services;
    }
}