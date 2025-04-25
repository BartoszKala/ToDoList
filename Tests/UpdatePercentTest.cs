using FluentAssertions;
using ToDoList.Application.ToDo;
using ToDoList.Infrastructure;
using ToDoList.Domain;
using Microsoft.EntityFrameworkCore;

namespace ToDoList.Application.Tests.ToDo
{
    public class UpdatePercentTest
    {
        private readonly DataContext _context;
        private readonly UpdatePercent.Handler _handler;

        public UpdatePercentTest()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new DataContext(options);
            _handler = new UpdatePercent.Handler(_context);
        }

        private async Task<Guid> SeedToDoItemAsync(int percent)
        {
            var item = new ToDoItem
            {
                Id = Guid.NewGuid(),
                Title = "Test Task",
                TimeOfExpiry = DateTime.UtcNow.AddDays(1),
                PercentCompleted = percent
            };

            _context.ToDoList.Add(item);
            await _context.SaveChangesAsync();
            return item.Id;
        }

        [Fact]
        public async Task Handle_ShouldUpdatePercent_WhenValid()
        {
            // Arrange
            var id = await SeedToDoItemAsync(40);

            var command = new UpdatePercent.Command
            {
                Id = id,
                PercentCompleted = 75
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            var updatedItem = await _context.ToDoList.FindAsync(id);
            updatedItem!.PercentCompleted.Should().Be(75);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenNoChangeNeeded()
        {
            // Arrange
            var id = await SeedToDoItemAsync(60);

            var command = new UpdatePercent.Command
            {
                Id = id,
                PercentCompleted = 60
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            var item = await _context.ToDoList.FindAsync(id);
            item!.PercentCompleted.Should().Be(60);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenItemNotFound()
        {
            // Arrange
            var command = new UpdatePercent.Command
            {
                Id = Guid.NewGuid(),
                PercentCompleted = 80
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("ToDoItem not found.");
        }
    }
}
