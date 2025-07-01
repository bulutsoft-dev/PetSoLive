using AutoMapper;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using Petsolive.API.DTOs;
using PetSoLive.API.DTOs;

namespace Petsolive.API.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User <-> UserDto
        CreateMap<User, UserDto>();

        // Pet <-> PetDto
        CreateMap<Pet, PetDto>();

        // PetOwner <-> PetOwnerDto
        CreateMap<PetOwner, PetOwnerDto>()
            .ForMember(dest => dest.PetName, opt => opt.MapFrom(src => src.Pet.Name))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username));

        // Veterinarian <-> VeterinarianDto
        CreateMap<Veterinarian, VeterinarianDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // Admin <-> AdminDto
        CreateMap<Admin, AdminDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username));

        // Adoption <-> AdoptionDto
        CreateMap<Adoption, AdoptionDto>()
            .ForMember(dest => dest.PetName, opt => opt.MapFrom(src => src.Pet.Name))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // AdoptionRequest <-> AdoptionRequestDto
        CreateMap<AdoptionRequest, AdoptionRequestDto>()
            .ForMember(dest => dest.PetName, opt => opt.MapFrom(src => src.Pet.Name))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // HelpRequest <-> HelpRequestDto
        CreateMap<HelpRequest, HelpRequestDto>()
            .ForMember(dest => dest.EmergencyLevel, opt => opt.MapFrom(src => src.EmergencyLevel.ToString()))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // Comment <-> CommentDto
        CreateMap<Comment, CommentDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.VeterinarianName, opt => opt.MapFrom(src => src.Veterinarian != null ? src.Veterinarian.User.Username : null));

        // LostPetAd <-> LostPetAdDto
        CreateMap<LostPetAd, LostPetAdDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username));
    }
}