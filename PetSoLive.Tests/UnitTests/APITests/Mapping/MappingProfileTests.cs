using Xunit;
using AutoMapper;
using Petsolive.API.Mapping;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using Petsolive.API.DTOs;
using PetSoLive.API.DTOs;
using System;
using System.Collections.Generic;

namespace PetSoLive.Tests.UnitTests.APITests.Mapping
{
    public class MappingProfileTests
    {
        private readonly IMapper _mapper;

        public MappingProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void MappingProfile_Configuration_IsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            config.AssertConfigurationIsValid();
        }

        [Fact]
        public void User_To_UserDto_Maps_All_Properties()
        {
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                PhoneNumber = "1234567890",
                Address = "Test Address",
                DateOfBirth = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow,
                ProfileImageUrl = "img.png",
                Roles = new List<string> { "User" },
                City = "TestCity",
                District = "TestDistrict"
            };

            var dto = _mapper.Map<UserDto>(user);

            Assert.Equal(user.Id, dto.Id);
            Assert.Equal(user.Username, dto.Username);
            Assert.Equal(user.Email, dto.Email);
            Assert.Equal(user.PhoneNumber, dto.PhoneNumber);
            Assert.Equal(user.Address, dto.Address);
            Assert.Equal(user.DateOfBirth, dto.DateOfBirth);
            Assert.Equal(user.IsActive, dto.IsActive);
            Assert.Equal(user.CreatedDate, dto.CreatedDate);
            Assert.Equal(user.LastLoginDate, dto.LastLoginDate);
            Assert.Equal(user.ProfileImageUrl, dto.ProfileImageUrl);
            Assert.Equal(user.Roles, dto.Roles);
            Assert.Equal(user.City, dto.City);
            Assert.Equal(user.District, dto.District);
        }

        [Fact]
        public void Pet_To_PetDto_And_Back_Maps_All_Properties()
        {
            var pet = new Pet
            {
                Name = "Kedi",
                Species = "Cat",
                Breed = "Van",
                Gender = "Female",
                Age = 2
            };

            var dto = _mapper.Map<PetDto>(pet);
            var mappedBack = _mapper.Map<Pet>(dto);

            Assert.Equal(pet.Name, dto.Name);
            Assert.Equal(pet.Species, dto.Species);
            Assert.Equal(pet.Breed, dto.Breed);
            Assert.Equal(pet.Gender, dto.Gender);
            Assert.Equal(pet.Age, dto.Age);
            Assert.Equal(pet.Name, mappedBack.Name);
        }

        [Fact]
        public void PetOwner_To_PetOwnerDto_Maps_Nested_Properties()
        {
            var petOwner = new PetOwner
            {
                Pet = new Pet { Name = "Kedi" },
                User = new User { Username = "testuser" }
            };

            var dto = _mapper.Map<PetOwnerDto>(petOwner);

            Assert.Equal("Kedi", dto.PetName);
            Assert.Equal("testuser", dto.UserName);
        }

        [Fact]
        public void Veterinarian_To_VeterinarianDto_Maps_Status_And_UserName()
        {
            var vet = new Veterinarian
            {
                Id = 1,
                User = new User { Username = "vetuser" },
                Status = VeterinarianStatus.Approved,
                AppliedDate = DateTime.UtcNow
            };

            var dto = _mapper.Map<VeterinarianDto>(vet);

            Assert.Equal("vetuser", dto.UserName);
            Assert.Equal(VeterinarianStatus.Approved.ToString(), dto.Status);
        }

        [Fact]
        public void Admin_To_AdminDto_Maps_UserName()
        {
            var admin = new Admin
            {
                User = new User { Username = "adminuser" }
            };

            var dto = _mapper.Map<AdminDto>(admin);

            Assert.Equal("adminuser", dto.UserName);
        }

        [Fact]
        public void Adoption_To_AdoptionDto_Maps_Nested_And_Status()
        {
            var adoption = new Adoption
            {
                Pet = new Pet { Name = "Kedi" },
                User = new User { Username = "adopter" },
                Status = AdoptionStatus.Approved
            };

            var dto = _mapper.Map<AdoptionDto>(adoption);

            Assert.Equal("Kedi", dto.PetName);
            Assert.Equal("adopter", dto.UserName);
            Assert.Equal(AdoptionStatus.Approved.ToString(), dto.Status);
        }

        [Fact]
        public void AdoptionRequest_To_AdoptionRequestDto_Maps_Nested_And_Status()
        {
            var req = new AdoptionRequest
            {
                Pet = new Pet { Name = "Köpek" },
                User = new User { Username = "requester" },
                Status = AdoptionStatus.Pending
            };

            var dto = _mapper.Map<AdoptionRequestDto>(req);

            Assert.Equal("Köpek", dto.PetName);
            Assert.Equal("requester", dto.UserName);
            Assert.Equal(AdoptionStatus.Pending.ToString(), dto.Status);
        }

        [Fact]
        public void HelpRequest_To_HelpRequestDto_Maps_Enums_And_Nested()
        {
            var help = new HelpRequest
            {
                EmergencyLevel = EmergencyLevel.High,
                User = new User { Username = "helpuser" },
                Status = HelpRequestStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            var dto = _mapper.Map<HelpRequestDto>(help);

            Assert.Equal(EmergencyLevel.High.ToString(), dto.EmergencyLevel);
            Assert.Equal("helpuser", dto.UserName);
            Assert.Equal(HelpRequestStatus.Active.ToString(), dto.Status);
        }

        [Fact]
        public void Comment_To_CommentDto_Maps_Nested()
        {
            var comment = new Comment
            {
                User = new User { Username = "commenter" },
                Veterinarian = new Veterinarian { User = new User { Username = "vet" } }
            };

            var dto = _mapper.Map<CommentDto>(comment);

            Assert.Equal("commenter", dto.UserName);
            Assert.Equal("vet", dto.VeterinarianName);
        }

        [Fact]
        public void LostPetAd_To_LostPetAdDto_Maps_Nested_And_Dates()
        {
            var ad = new LostPetAd
            {
                User = new User { Username = "lostuser" },
                LastSeenDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            var dto = _mapper.Map<LostPetAdDto>(ad);

            Assert.Equal("lostuser", dto.UserName);
            Assert.Equal(ad.LastSeenDate, dto.LastSeenDate);
            Assert.Equal(ad.CreatedAt, dto.CreatedAt);
        }

        [Fact]
        public void AuthDto_To_User_Maps_Username_And_Password()
        {
            var authDto = new AuthDto { Username = "authuser", Password = "pass" };
            var user = _mapper.Map<User>(authDto);

            Assert.Equal("authuser", user.Username);
            Assert.Equal("pass", user.PasswordHash);
        }

        [Fact]
        public void RegisterDto_To_User_Maps_Username_And_Password()
        {
            var regDto = new RegisterDto { Username = "reguser", Password = "regpass", DateOfBirth = new DateTime(1990, 1, 1) };
            var user = _mapper.Map<User>(regDto);

            Assert.Equal("reguser", user.Username);
            Assert.Equal("regpass", user.PasswordHash);
            Assert.Equal(DateTimeKind.Utc, user.DateOfBirth.Kind);
        }

        [Fact]
        public void User_To_AuthResponseDto_Maps_User()
        {
            var user = new User { Username = "authresp" };
            var resp = _mapper.Map<AuthResponseDto>(user);

            Assert.NotNull(resp.User);
            Assert.Equal("authresp", resp.User.Username);
        }
    }
} 