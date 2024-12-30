using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using PetSoLive.Business.Services;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;

namespace PetSoLive.Tests.UnitTests;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
        private readonly CommentService _commentService;

        public CommentServiceTests()
        {
            _commentRepositoryMock = new Mock<ICommentRepository>();
            _commentService = new CommentService(_commentRepositoryMock.Object);
        }

        [Fact]
        public async Task AddCommentAsync_WhenCalled_InvokesRepositoryWithCorrectComment()
        {
            // Arrange
            var newComment = new Comment
            {
                Id = 1,
                HelpRequestId = 10,
                Content = "Test comment",
                UserId = 123
            };

            _commentRepositoryMock
                .Setup(repo => repo.AddCommentAsync(newComment))
                .Returns(Task.CompletedTask);

            // Act
            await _commentService.AddCommentAsync(newComment);

            // Assert
            _commentRepositoryMock.Verify(
                repo => repo.AddCommentAsync(It.Is<Comment>(c => c == newComment)),
                Times.Once
            );
        }

        [Fact]
        public async Task GetCommentsByHelpRequestIdAsync_WhenCalled_ReturnsCommentsList()
        {
            // Arrange
            var helpRequestId = 10;
            var comments = new List<Comment>
            {
                new Comment { Id = 1, HelpRequestId = helpRequestId, Content = "Comment 1" },
                new Comment { Id = 2, HelpRequestId = helpRequestId, Content = "Comment 2" }
            };

            _commentRepositoryMock
                .Setup(repo => repo.GetCommentsByHelpRequestIdAsync(helpRequestId))
                .ReturnsAsync(comments);

            // Act
            var result = await _commentService.GetCommentsByHelpRequestIdAsync(helpRequestId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Comment 1", result[0].Content);
            Assert.Equal("Comment 2", result[1].Content);

            _commentRepositoryMock.Verify(
                repo => repo.GetCommentsByHelpRequestIdAsync(helpRequestId),
                Times.Once
            );
        }

        [Fact]
        public async Task GetCommentByIdAsync_WhenCalled_ReturnsComment()
        {
            // Arrange
            var commentId = 5;
            var expectedComment = new Comment
            {
                Id = commentId,
                Content = "Expected comment",
                UserId = 123
            };

            _commentRepositoryMock
                .Setup(repo => repo.GetCommentByIdAsync(commentId))
                .ReturnsAsync(expectedComment);

            // Act
            var result = await _commentService.GetCommentByIdAsync(commentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(commentId, result.Id);
            Assert.Equal("Expected comment", result.Content);

            _commentRepositoryMock.Verify(
                repo => repo.GetCommentByIdAsync(commentId),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdateCommentAsync_WhenCalled_InvokesRepositoryWithCorrectComment()
        {
            // Arrange
            var existingComment = new Comment
            {
                Id = 2,
                Content = "Original content",
                UserId = 123
            };

            // Simulating that the repository will handle the update
            _commentRepositoryMock
                .Setup(repo => repo.UpdateCommentAsync(existingComment))
                .Returns(Task.CompletedTask);

            // Act
            await _commentService.UpdateCommentAsync(existingComment);

            // Assert
            _commentRepositoryMock.Verify(
                repo => repo.UpdateCommentAsync(It.Is<Comment>(c => c == existingComment)),
                Times.Once
            );
        }

        [Fact]
        public async Task DeleteCommentAsync_WhenCalled_InvokesRepositoryWithCorrectId()
        {
            // Arrange
            var commentIdToDelete = 3;

            _commentRepositoryMock
                .Setup(repo => repo.DeleteCommentAsync(commentIdToDelete))
                .Returns(Task.CompletedTask);

            // Act
            await _commentService.DeleteCommentAsync(commentIdToDelete);

            // Assert
            _commentRepositoryMock.Verify(
                repo => repo.DeleteCommentAsync(commentIdToDelete),
                Times.Once
            );
        } 
}
