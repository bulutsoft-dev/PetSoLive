using Xunit;
using Moq;
using Petsolive.API.Controllers;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using PetSoLive.API.DTOs;
using PetSoLive.Core.Entities;
using Microsoft.AspNetCore.Http;
using Petsolive.API.DTOs;

namespace PetSoLive.Tests.UnitTests.APITests.Controllers
{
    public class PetControllerTests
    {
        private readonly Mock<IServiceManager> _serviceManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly PetController _controller;

        public PetControllerTests()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _mapperMock = new Mock<IMapper>();
            _emailServiceMock = new Mock<IEmailService>();
            _serviceManagerMock.SetupGet(s => s.EmailService).Returns(_emailServiceMock.Object);
            _controller = new PetController(_serviceManagerMock.Object, _mapperMock.Object);
        }

        private void SetUser(int userId)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithListOfPetDto()
        {
            var pets = new List<Pet> { new Pet { Id = 1 }, new Pet { Id = 2 } };
            var petDtos = new List<PetDto> { new PetDto { Id = 1 }, new PetDto { Id = 2 } };
            _serviceManagerMock.Setup(s => s.PetService.GetAllPetsAsync()).ReturnsAsync(pets);
            _mapperMock.Setup(m => m.Map<IEnumerable<PetDto>>(pets)).Returns(petDtos);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(petDtos, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenPetExists()
        {
            int id = 1;
            var pet = new Pet { Id = id };
            var petDto = new PetDto { Id = id };
            _serviceManagerMock.Setup(s => s.PetService.GetPetByIdAsync(id)).ReturnsAsync(pet);
            _mapperMock.Setup(m => m.Map<PetDto>(pet)).Returns(petDto);

            var result = await _controller.GetById(id);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(petDto, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenPetDoesNotExist()
        {
            int id = 2;
            _serviceManagerMock.Setup(s => s.PetService.GetPetByIdAsync(id)).ReturnsAsync((Pet)null);

            var result = await _controller.GetById(id);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_OnSuccess()
        {
            var petDto = new PetDto { Id = 0 };
            var pet = new Pet { Id = 1 };
            var petDtoResult = new PetDto { Id = 1 };
            SetUser(5);

            _mapperMock.Setup(m => m.Map<Pet>(petDto)).Returns(pet);
            _mapperMock.Setup(m => m.Map<PetDto>(pet)).Returns(petDtoResult);
            _serviceManagerMock.Setup(s => s.PetService.CreatePetAsync(pet)).Returns(Task.CompletedTask);
            _serviceManagerMock.Setup(s => s.PetService.AssignPetOwnerAsync(It.IsAny<PetOwner>())).Returns(Task.CompletedTask);

            var result = await _controller.Create(petDto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
            Assert.Equal(petDtoResult, createdResult.Value);
        }

        [Fact]
        public async Task Create_ReturnsUnauthorized_WhenUserIdMissing()
        {
            var petDto = new PetDto { Id = 0 };
            var pet = new Pet { Id = 1 };
            _mapperMock.Setup(m => m.Map<Pet>(petDto)).Returns(pet);
            _serviceManagerMock.Setup(s => s.PetService.CreatePetAsync(pet)).Returns(Task.CompletedTask);

            // No user set
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await _controller.Create(petDto);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal("Kullanıcı kimliği bulunamadı.", unauthorized.Value);
        }

        [Fact]
        public async Task Create_SendsEmailToOwner()
        {
            var petDto = new PetDto { Id = 0 };
            var pet = new Pet { Id = 1 };
            SetUser(5);
            var user = new User { Id = 5, Email = "owner@mail.com" };
            _mapperMock.Setup(m => m.Map<Pet>(petDto)).Returns(pet);
            _serviceManagerMock.Setup(s => s.PetService.CreatePetAsync(pet)).Returns(Task.CompletedTask);
            _serviceManagerMock.Setup(s => s.PetService.AssignPetOwnerAsync(It.IsAny<PetOwner>())).Returns(Task.CompletedTask);
            _serviceManagerMock.Setup(s => s.UserService.GetUserByIdAsync(5)).ReturnsAsync(user);
            _mapperMock.Setup(m => m.Map<PetDto>(pet)).Returns(new PetDto { Id = 1 });
            var result = await _controller.Create(petDto);
            _emailServiceMock.Verify(e => e.SendEmailAsync("owner@mail.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_OnSuccess()
        {
            int id = 1;
            var petDto = new PetDto { Id = id };
            var pet = new Pet { Id = id };
            SetUser(5);

            _mapperMock.Setup(m => m.Map<Pet>(petDto)).Returns(pet);
            _serviceManagerMock.Setup(s => s.PetService.IsUserOwnerOfPetAsync(id, 5)).ReturnsAsync(true);
            _serviceManagerMock.Setup(s => s.PetService.UpdatePetAsync(id, pet, 5)).Returns(Task.CompletedTask);

            var result = await _controller.Update(id, petDto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsForbid_WhenUserIsNotOwner()
        {
            int id = 1;
            var petDto = new PetDto { Id = id };
            var pet = new Pet { Id = id };
            SetUser(5);

            _mapperMock.Setup(m => m.Map<Pet>(petDto)).Returns(pet);
            _serviceManagerMock.Setup(s => s.PetService.IsUserOwnerOfPetAsync(id, 5)).ReturnsAsync(false);

            var result = await _controller.Update(id, petDto);

            var forbid = Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsUnauthorized_WhenUserIdMissing()
        {
            int id = 1;
            var petDto = new PetDto { Id = id };
            var pet = new Pet { Id = id };
            _mapperMock.Setup(m => m.Map<Pet>(petDto)).Returns(pet);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await _controller.Update(id, petDto);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Kullanıcı kimliği bulunamadı.", unauthorized.Value);
        }

        [Fact]
        public async Task Update_SendsEmailToAdoptionRequestUsers()
        {
            int id = 1;
            var petDto = new PetDto { Id = id };
            var pet = new Pet { Id = id };
            SetUser(5);
            var adoptionRequests = new List<AdoptionRequest> {
                new AdoptionRequest { User = new User { Email = "user1@mail.com" } },
                new AdoptionRequest { User = new User { Email = "user2@mail.com" } }
            };
            _mapperMock.Setup(m => m.Map<Pet>(petDto)).Returns(pet);
            _serviceManagerMock.Setup(s => s.PetService.IsUserOwnerOfPetAsync(id, 5)).ReturnsAsync(true);
            _serviceManagerMock.Setup(s => s.PetService.UpdatePetAsync(id, pet, 5)).Returns(Task.CompletedTask);
            _serviceManagerMock.Setup(s => s.AdoptionRequestService.GetAdoptionRequestsByPetIdAsync(id)).ReturnsAsync(adoptionRequests);
            var result = await _controller.Update(id, petDto);
            _emailServiceMock.Verify(e => e.SendEmailAsync("user1@mail.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _emailServiceMock.Verify(e => e.SendEmailAsync("user2@mail.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_OnSuccess()
        {
            int id = 1;
            SetUser(5);

            _serviceManagerMock.Setup(s => s.PetService.IsUserOwnerOfPetAsync(id, 5)).ReturnsAsync(true);
            _serviceManagerMock.Setup(s => s.PetService.DeletePetAsync(id, 5)).Returns(Task.CompletedTask);

            var result = await _controller.Delete(id);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsForbidden_WhenUserIsNotOwner()
        {
            int id = 1;
            SetUser(5);

            _serviceManagerMock.Setup(s => s.PetService.IsUserOwnerOfPetAsync(id, 5)).ReturnsAsync(false);

            var result = await _controller.Delete(id);

            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbidden.StatusCode);
        }

        [Fact]
        public async Task Delete_ReturnsUnauthorized_WhenUserIdMissing()
        {
            int id = 1;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await _controller.Delete(id);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Kullanıcı kimliği bulunamadı.", unauthorized.Value);
        }

        [Fact]
        public async Task Delete_SendsEmailToAdoptionRequestUsers()
        {
            int id = 1;
            SetUser(5);
            var adoptionRequests = new List<AdoptionRequest> {
                new AdoptionRequest { User = new User { Email = "user1@mail.com" } },
                new AdoptionRequest { User = new User { Email = "user2@mail.com" } }
            };
            var pet = new Pet { Id = id };
            _serviceManagerMock.Setup(s => s.PetService.IsUserOwnerOfPetAsync(id, 5)).ReturnsAsync(true);
            _serviceManagerMock.Setup(s => s.PetService.DeletePetAsync(id, 5)).Returns(Task.CompletedTask);
            _serviceManagerMock.Setup(s => s.AdoptionRequestService.GetAdoptionRequestsByPetIdAsync(id)).ReturnsAsync(adoptionRequests);
            _serviceManagerMock.Setup(s => s.PetService.GetPetByIdAsync(id)).ReturnsAsync(pet);
            var result = await _controller.Delete(id);
            _emailServiceMock.Verify(e => e.SendEmailAsync("user1@mail.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _emailServiceMock.Verify(e => e.SendEmailAsync("user2@mail.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
} 