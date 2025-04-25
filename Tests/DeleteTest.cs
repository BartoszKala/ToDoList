using ToDoList.Application.ToDo;
using ToDoList.Domain;
using ToDoList.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;


namespace ToDoList.Application.Tests.ToDo
{
    public class DeleteTests
    {
        private readonly Mock<DataContext> _mockContext;
        private readonly Delete.Handler _handler;

        public DeleteTests()
        {
            _mockContext = new Mock<DataContext>(new DbContextOptions<DataContext>());
            _handler = new Delete.Handler(_mockContext.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenItemExistsAndDeleted()
        {
            // Arrange
            var toDoItem = new ToDoItem { Id = Guid.NewGuid(), Title = "Test", TimeOfExpiry=DateTime.Now,PercentCompleted=1 };
            var mockDbSet = new Mock<DbSet<ToDoItem>>();

            mockDbSet.Setup(x => x.FindAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(toDoItem);

            _mockContext.Setup(x => x.ToDoList)
                      .Returns(mockDbSet.Object);

            _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

            var command = new Delete.Command { Id = toDoItem.Id };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _mockContext.Verify(x => x.Remove(toDoItem), Times.Once);
            _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenItemNotFound()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<ToDoItem>>();

            mockDbSet.Setup(x => x.FindAsync(It.IsAny<Guid>()))
                    .ReturnsAsync((ToDoItem)null);

            _mockContext.Setup(x => x.ToDoList)
                      .Returns(mockDbSet.Object);

            var command = new Delete.Command { Id = Guid.NewGuid() };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("ToDo item not found");
            _mockContext.Verify(x => x.Remove(It.IsAny<ToDoItem>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenSaveChangesFails()
        {
            // Arrange
            var toDoItem = new ToDoItem { Id = Guid.NewGuid(), Title = "Test", PercentCompleted=1, TimeOfExpiry=DateTime.Now };
            var mockDbSet = new Mock<DbSet<ToDoItem>>();

            mockDbSet.Setup(x => x.FindAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(toDoItem);

            _mockContext.Setup(x => x.ToDoList)
                      .Returns(mockDbSet.Object);

            _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(0);

            var command = new Delete.Command { Id = toDoItem.Id };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Failed to delete the ToDo");
            _mockContext.Verify(x => x.Remove(toDoItem), Times.Once);
        }
    }
}