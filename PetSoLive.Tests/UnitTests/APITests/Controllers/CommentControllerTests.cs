using Xunit;
using Moq;
using Petsolive.API.Controllers;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Petsolive.API.DTOs;
using PetSoLive.Core.Entities;

namespace PetSoLive.Tests.UnitTests.APITests.Controllers
{
    public class CommentControllerTests
    {
        private readonly Mock<IServiceManager> _serviceManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CommentController _controller;

        public CommentControllerTests()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _mapperMock = new Mock<IMapper>();
            _controller = new CommentController(_serviceManagerMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetByHelpRequestId_ReturnsOk_WithListOfCommentDto()
        {
            // Arrange
            int helpRequestId = 1;
            var comments = new List<Comment> { new Comment { Id = 1 }, new Comment { Id = 2 } };
            var commentDtos = new List<CommentDto> { new CommentDto { Id = 1 }, new CommentDto { Id = 2 } };
            _serviceManagerMock.Setup(s => s.CommentService.GetCommentsByHelpRequestIdAsync(helpRequestId)).ReturnsAsync(comments);
            _mapperMock.Setup(m => m.Map<IEnumerable<CommentDto>>(comments)).Returns(commentDtos);

            // Act
            var result = await _controller.GetByHelpRequestId(helpRequestId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(commentDtos, okResult.Value);
        }

        [Fact]
        public async Task Add_ReturnsOk_OnSuccess()
        {
            // Arrange
            var dto = new CommentDto { Id = 1 };
            var entity = new Comment { Id = 1 };
            _mapperMock.Setup(m => m.Map<Comment>(dto)).Returns(entity);
            _serviceManagerMock.Setup(s => s.CommentService.AddCommentAsync(entity)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Add(dto);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Add_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var dto = new CommentDto();
            _controller.ModelState.AddModelError("Text", "Required");

            // Act
            var result = await _controller.Add(dto);

            // Assert
            Assert.IsType<BadRequestResult>(result); // Controller'da bu yok, eklenirse bu test çalışır
        }

        [Fact]
        public async Task Add_ThrowsException_WhenServiceFails()
        {
            // Arrange
            var dto = new CommentDto { Id = 1 };
            var entity = new Comment { Id = 1 };
            _mapperMock.Setup(m => m.Map<Comment>(dto)).Returns(entity);
            _serviceManagerMock.Setup(s => s.CommentService.AddCommentAsync(entity)).ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Add(dto));
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_OnSuccess()
        {
            // Arrange
            int id = 1;
            _serviceManagerMock.Setup(s => s.CommentService.DeleteCommentAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ThrowsException_WhenServiceFails()
        {
            // Arrange
            int id = 2;
            _serviceManagerMock.Setup(s => s.CommentService.DeleteCommentAsync(id)).ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Delete(id));
        }
    }
} 