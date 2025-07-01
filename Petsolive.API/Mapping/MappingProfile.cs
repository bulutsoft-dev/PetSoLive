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
// User -> UserDto (şifre alanını ignore et)
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src =>
                src.DateOfBirth != default
                    ? DateTime.SpecifyKind(src.DateOfBirth, DateTimeKind.Utc)
                    : default(DateTime)
            ))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src =>
                src.CreatedDate != default
                    ? DateTime.SpecifyKind(src.CreatedDate, DateTimeKind.Utc)
                    : default(DateTime)
            ))
            .ForMember(dest => dest.LastLoginDate, opt => opt.MapFrom(src =>
                src.LastLoginDate.HasValue
                    ? DateTime.SpecifyKind(src.LastLoginDate.Value, DateTimeKind.Utc)
                    : (DateTime?)null
            ));

        // UserDto -> User (şifre alanını ignore et)
        CreateMap<UserDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src =>
                src.DateOfBirth != default
                    ? DateTime.SpecifyKind(src.DateOfBirth, DateTimeKind.Utc)
                    : default(DateTime)
            ))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src =>
                src.CreatedDate != default
                    ? DateTime.SpecifyKind(src.CreatedDate, DateTimeKind.Utc)
                    : default(DateTime)
            ))
            .ForMember(dest => dest.LastLoginDate, opt => opt.MapFrom(src =>
                src.LastLoginDate.HasValue
                    ? DateTime.SpecifyKind(src.LastLoginDate.Value, DateTimeKind.Utc)
                    : (DateTime?)null
            ));

        // Pet <-> PetDto
        CreateMap<Pet, PetDto>().ReverseMap()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Create işlemlerinde Id'yi ignore et
            .ForMember(dest => dest.AdoptionRequests, opt => opt.Ignore())
            .ForMember(dest => dest.PetOwners, opt => opt.Ignore());

        // PetOwner <-> PetOwnerDto
        CreateMap<PetOwner, PetOwnerDto>()
            .ForMember(dest => dest.PetName, opt => opt.MapFrom(src => src.Pet.Name))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
            .ReverseMap()
            .ForMember(dest => dest.Pet, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());

        // Veterinarian <-> VeterinarianDto
        CreateMap<Veterinarian, VeterinarianDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ReverseMap()
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<VeterinarianStatus>(src.Status)))
            .ForMember(dest => dest.Comments, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // Admin <-> AdminDto
        CreateMap<Admin, AdminDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
            .ReverseMap()
            .ForMember(dest => dest.User, opt => opt.Ignore());

        // Adoption <-> AdoptionDto
        CreateMap<Adoption, AdoptionDto>()
            .ForMember(dest => dest.PetName, opt => opt.MapFrom(src => src.Pet.Name))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ReverseMap()
            .ForMember(dest => dest.Pet, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<AdoptionStatus>(src.Status)))
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // AdoptionRequest <-> AdoptionRequestDto
        CreateMap<AdoptionRequest, AdoptionRequestDto>()
            .ForMember(dest => dest.PetName, opt => opt.MapFrom(src => src.Pet.Name))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ReverseMap()
            .ForMember(dest => dest.Pet, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<AdoptionStatus>(src.Status)))
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // HelpRequest <-> HelpRequestDto
        CreateMap<HelpRequest, HelpRequestDto>()
            .ForMember(dest => dest.EmergencyLevel, opt => opt.MapFrom(src => src.EmergencyLevel.ToString()))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ReverseMap()
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.EmergencyLevel, opt => opt.MapFrom(src => Enum.Parse<EmergencyLevel>(src.EmergencyLevel)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<HelpRequestStatus>(src.Status)))
            .ForMember(dest => dest.Comments, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // Comment <-> CommentDto
        CreateMap<Comment, CommentDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.VeterinarianName, opt => opt.MapFrom(src => src.Veterinarian != null ? src.Veterinarian.User.Username : null))
            .ReverseMap()
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Veterinarian, opt => opt.Ignore())
            .ForMember(dest => dest.HelpRequest, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // LostPetAd <-> LostPetAdDto
        CreateMap<LostPetAd, LostPetAdDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
            .ReverseMap()
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());

            // LostPetAdDto -> LostPetAd
CreateMap<LostPetAdDto, LostPetAd>()
    .ForMember(dest => dest.User, opt => opt.Ignore())
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.LastSeenDate, opt => opt.MapFrom(src =>
        src.LastSeenDate.Kind == DateTimeKind.Utc
            ? src.LastSeenDate
            : DateTime.SpecifyKind(src.LastSeenDate, DateTimeKind.Utc)))
    .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src =>
        src.CreatedAt.Kind == DateTimeKind.Utc
            ? src.CreatedAt
            : DateTime.SpecifyKind(src.CreatedAt, DateTimeKind.Utc)));

        // AuthDto -> User (for login)
        CreateMap<AuthDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
            .ForMember(dest => dest.Address, opt => opt.Ignore())
            .ForMember(dest => dest.DateOfBirth, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastLoginDate, opt => opt.Ignore())
            .ForMember(dest => dest.ProfileImageUrl, opt => opt.Ignore())
            .ForMember(dest => dest.Roles, opt => opt.Ignore())
            .ForMember(dest => dest.City, opt => opt.Ignore())
            .ForMember(dest => dest.District, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // RegisterDto -> User (for registration)
        CreateMap<RegisterDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src =>
                src.DateOfBirth != default
                    ? DateTime.SpecifyKind(src.DateOfBirth, DateTimeKind.Utc)
                    : default(DateTime)
            ))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastLoginDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.Roles, opt => opt.Ignore());

        // AuthResponseDto <-> User (for login response)
        CreateMap<User, AuthResponseDto>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src));

        // LostPetAdFilterDto: Sadece filtreleme için, entity map gerekmez.
    }
}