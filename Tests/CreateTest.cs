using ToDoList.Application.ToDo;
using ToDoList.Domain;
using ToDoList.Infrastructure;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Moq;

namespace ToDoList.Application.Tests.ToDo
{
    public class CreateTests
    {
        private readonly DataContext _context;
        private readonly Create.Handler _handler;
        private readonly Create.CommandValidator _validator;

        public CreateTests()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DataContext(options);
            _handler = new Create.Handler(_context);
            _validator = new Create.CommandValidator();
        }

        private ToDoItem CreateTestToDoItem(string title = "Test title", string description = "Test description")
        {
            return new ToDoItem
            {
                Id = Guid.NewGuid(), 
                Title = title,
                Description = description,
                TimeOfExpiry = DateTime.UtcNow,
                PercentCompleted= 0
            };
        }

        [Fact]
        public void Validator_ShouldHaveError_WhenTitleIsEmpty()
        {
            // Arrange
            var command = new Create.Command
            {
                toDo = CreateTestToDoItem(title: "")
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.toDo.Title);
        }

        [Fact]
        public void Validator_ShouldHaveError_WhenTitleIsTooLong()
        {
            // Arrange
            var longTitle = new string('a', 101); 
            var command = new Create.Command
            {
                toDo = CreateTestToDoItem(title: longTitle)
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.toDo.Title);
        }

        [Fact]
        public void Validator_ShouldNotHaveError_WhenTitleIsValid()
        {
            // Arrange
            var command = new Create.Command
            {
                toDo = CreateTestToDoItem()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.toDo.Title);
        }

        [Fact]
        public async Task Handler_ShouldAddToDoItem_WhenValid()
        {
            // Arrange
            var command = new Create.Command
            {
                toDo = CreateTestToDoItem()
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            var itemInDb = await _context.ToDoList.FirstOrDefaultAsync();
            Assert.NotNull(itemInDb);
            Assert.Equal("Test title", itemInDb.Title);
        }

        [Fact]
        public async Task Handler_ShouldReturnSuccess_WhenSavingChanges()
        {
            // Arrange
            var command = new Create.Command
            {
                toDo = CreateTestToDoItem()
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(Unit.Value, result.Value);
        }

        [Fact]
         public async Task Handler_ShouldReturnFailure_WhenDbContextThrowsException()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "ThrowExceptionDb")
                .Options;

            var mockSet = new Mock<DbSet<ToDoItem>>();
            mockSet.Setup(m => m.Add(It.IsAny<ToDoItem>()))
                   .Throws(new DbUpdateException("Test exception"));

            var mockContext = new Mock<DataContext>(options);
            mockContext.Setup(m => m.ToDoList).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new DbUpdateException("Test exception"));

            var handler = new Create.Handler(mockContext.Object);
            var command = new Create.Command
            {
                toDo = CreateTestToDoItem()
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(() =>
                handler.Handle(command, CancellationToken.None));
        }
    }
}