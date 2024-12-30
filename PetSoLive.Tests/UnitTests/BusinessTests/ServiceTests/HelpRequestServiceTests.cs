using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using PetSoLive.Business.Services;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Core.Interfaces;

namespace PetSoLive.Tests.UnitTests;

public class HelpRequestServiceTests
{
    private readonly Mock<IHelpRequestRepository> _helpRequestRepositoryMock;
        private readonly HelpRequestService _helpRequestService;

        public HelpRequestServiceTests()
        {
            _helpRequestRepositoryMock = new Mock<IHelpRequestRepository>();
            _helpRequestService = new HelpRequestService(_helpRequestRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateHelpRequestAsync_WhenCalled_SetsStatusToActiveAndCallsRepository()
        {
            // Arrange
            var newHelpRequest = new HelpRequest
            {
                Id = 1,
                Title = "Need help",
                UserId = 123
                // Status will be set automatically to Active in the service
            };

            _helpRequestRepositoryMock
                .Setup(repo => repo.CreateHelpRequestAsync(It.IsAny<HelpRequest>()))
                .Returns(Task.CompletedTask);

            // Act
            await _helpRequestService.CreateHelpRequestAsync(newHelpRequest);

            // Assert
            Assert.Equal(HelpRequestStatus.Active, newHelpRequest.Status);

            _helpRequestRepositoryMock.Verify(
                repo => repo.CreateHelpRequestAsync(It.Is<HelpRequest>(
                    hr => hr.Id == 1 &&
                          hr.Title == "Need help" &&
                          hr.Status == HelpRequestStatus.Active)
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetHelpRequestsAsync_WhenCalled_ReturnsListOfHelpRequests()
        {
            // Arrange
            var helpRequests = new List<HelpRequest>
            {
                new HelpRequest { Id = 1, Title = "HelpRequest 1" },
                new HelpRequest { Id = 2, Title = "HelpRequest 2" }
            };

            _helpRequestRepositoryMock
                .Setup(repo => repo.GetHelpRequestsAsync())
                .ReturnsAsync(helpRequests);

            // Act
            var result = await _helpRequestService.GetHelpRequestsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("HelpRequest 1", result[0].Title);
            Assert.Equal("HelpRequest 2", result[1].Title);

            _helpRequestRepositoryMock.Verify(
                repo => repo.GetHelpRequestsAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task GetHelpRequestByIdAsync_WhenCalled_ReturnsSingleHelpRequest()
        {
            // Arrange
            var helpRequestId = 5;
            var expectedHelpRequest = new HelpRequest
            {
                Id = helpRequestId,
                Title = "Test Request"
            };

            _helpRequestRepositoryMock
                .Setup(repo => repo.GetHelpRequestByIdAsync(helpRequestId))
                .ReturnsAsync(expectedHelpRequest);

            // Act
            var result = await _helpRequestService.GetHelpRequestByIdAsync(helpRequestId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(helpRequestId, result.Id);
            Assert.Equal("Test Request", result.Title);

            _helpRequestRepositoryMock.Verify(
                repo => repo.GetHelpRequestByIdAsync(helpRequestId),
                Times.Once
            );
        }

        [Fact]
        public async Task GetHelpRequestsByUserAsync_WhenCalled_ReturnsListOfHelpRequests()
        {
            // Arrange
            var userId = 123;
            var userHelpRequests = new List<HelpRequest>
            {
                new HelpRequest { Id = 10, Title = "User Request 1", UserId = userId },
                new HelpRequest { Id = 11, Title = "User Request 2", UserId = userId }
            };

            _helpRequestRepositoryMock
                .Setup(repo => repo.GetHelpRequestsByUserAsync(userId))
                .ReturnsAsync(userHelpRequests);

            // Act
            var result = await _helpRequestService.GetHelpRequestsByUserAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(userId, result[0].UserId);
            Assert.Equal(userId, result[1].UserId);

            _helpRequestRepositoryMock.Verify(
                repo => repo.GetHelpRequestsByUserAsync(userId),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdateHelpRequestAsync_WhenCalled_UpdatesHelpRequest()
        {
            // Arrange
            var existingHelpRequest = new HelpRequest
            {
                Id = 2,
                Title = "Original Title",
                Status = HelpRequestStatus.Active
            };

            _helpRequestRepositoryMock
                .Setup(repo => repo.UpdateHelpRequestAsync(existingHelpRequest))
                .Returns(Task.CompletedTask);

            // Act
            await _helpRequestService.UpdateHelpRequestAsync(existingHelpRequest);

            // Assert
            _helpRequestRepositoryMock.Verify(
                repo => repo.UpdateHelpRequestAsync(It.Is<HelpRequest>(
                    hr => hr.Id == 2 && hr.Title == "Original Title"
                )),
                Times.Once
            );
        }

        [Fact]
        public async Task DeleteHelpRequestAsync_WhenCalled_DeletesHelpRequestById()
        {
            // Arrange
            var helpRequestId = 3;

            _helpRequestRepositoryMock
                .Setup(repo => repo.DeleteHelpRequestAsync(helpRequestId))
                .Returns(Task.CompletedTask);

            // Act
            await _helpRequestService.DeleteHelpRequestAsync(helpRequestId);

            // Assert
            _helpRequestRepositoryMock.Verify(
                repo => repo.DeleteHelpRequestAsync(helpRequestId),
                Times.Once
            );
        }
}