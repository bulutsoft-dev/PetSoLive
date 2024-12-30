using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

// Adjust namespaces to match your project structure
using PetSoLive.Core.Entities;
using PetSoLive.Data;
using PetSoLive.Core.Interfaces;
using PetSoLive.Data.Repositories;

namespace PetSoLive.Tests.UnitTests.DataTests.RepositoriesTests;

public class CommentRepositoryTests
{
    // <summary>
        /// Creates a new in-memory ApplicationDbContext for each test.
        /// </summary>
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unique DB name per test
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task AddCommentAsync_WhenCalled_InsertsCommentIntoDatabase()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            ICommentRepository repository = new CommentRepository(context);

            var newComment = new Comment
            {
                Id = 1,
                HelpRequestId = 10,
                UserId = 100,
                Content = "Test comment",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await repository.AddCommentAsync(newComment);

            // Assert
            var insertedComment = await context.Comments.FindAsync(newComment.Id);
            Assert.NotNull(insertedComment);
            Assert.Equal(10, insertedComment.HelpRequestId);
            Assert.Equal(100, insertedComment.UserId);
            Assert.Equal("Test comment", insertedComment.Content);
        }

        

        [Fact]
        public async Task UpdateCommentAsync_WhenCalled_UpdatesCommentInDatabase()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            ICommentRepository repository = new CommentRepository(context);

            var comment = new Comment
            {
                Id = 10,
                HelpRequestId = 99,
                UserId = 999,
                Content = "Original comment",
                CreatedAt = DateTime.UtcNow
            };

            context.Comments.Add(comment);
            await context.SaveChangesAsync();

            // Act
            comment.Content = "Updated comment";
            await repository.UpdateCommentAsync(comment);

            // Assert
            var updated = await context.Comments.FindAsync(10);
            Assert.NotNull(updated);
            Assert.Equal("Updated comment", updated.Content);
        }

        [Fact]
        public async Task DeleteCommentAsync_WhenCommentExists_DeletesComment()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            ICommentRepository repository = new CommentRepository(context);

            var comment = new Comment
            {
                Id = 7,
                HelpRequestId = 88,
                UserId = 888,
                Content = "Comment to delete",
                CreatedAt = DateTime.UtcNow
            };

            context.Comments.Add(comment);
            await context.SaveChangesAsync();

            // Act
            await repository.DeleteCommentAsync(comment.Id);

            // Assert
            var deletedComment = await context.Comments.FindAsync(comment.Id);
            Assert.Null(deletedComment);
        }

        [Fact]
        public async Task DeleteCommentAsync_WhenCommentNotFound_NoExceptionThrown()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            ICommentRepository repository = new CommentRepository(context);

            // No comment with Id=999 in the database

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => repository.DeleteCommentAsync(999));
            Assert.Null(exception); // Should not throw, just does nothing
        }
}