using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ToDoList.Application.ToDo;
using ToDoList.Domain;
using ToDoList.Infrastructure;


namespace ToDoList.Application.Tests.ToDo
{
    public class SpecificToDoTest
    {
        private readonly Mock<DataContext> _mockContext;
        private readonly SpecificToDo.Handler _handler;

        public SpecificToDoTest()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) 
                .Options;

            var context = new DataContext(options);
            _mockContext = new Mock<DataContext>(options) { CallBase = true }; 
            _handler = new SpecificToDo.Handler(_mockContext.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnToDoItem_WhenItemExists()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var testItem = new ToDoItem
            {
                Id = itemId,
                Title = "Test Task",
                Description = "Testing...",
                PercentCompleted = 50,
                TimeOfExpiry = DateTime.Now.AddDays(1)
            };

            var dbSetMock = CreateMockDbSet(new List<ToDoItem> { testItem }.AsQueryable());
            _mockContext.Setup(x => x.ToDoList).Returns(dbSetMock.Object);
            _mockContext.Setup(x => x.ToDoList.FindAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(testItem);

            var query = new SpecificToDo.Query { Id = itemId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(testItem);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenItemDoesNotExist()
        {
            // Arrange
            _mockContext.Setup(x => x.ToDoList.FindAsync(It.IsAny<Guid>()))
                        .ReturnsAsync((ToDoItem?)null);

            var query = new SpecificToDo.Query { Id = Guid.NewGuid() };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("ToDo item not found.");
        }

        private static Mock<DbSet<ToDoItem>> CreateMockDbSet(IQueryable<ToDoItem> data)
        {
            var mockSet = new Mock<DbSet<ToDoItem>>();
            mockSet.As<IQueryable<ToDoItem>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<ToDoItem>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<ToDoItem>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<ToDoItem>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            return mockSet;
        }
    }
}
