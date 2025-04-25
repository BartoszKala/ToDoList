using ToDoList.Application.ToDo;
using ToDoList.Infrastructure;
using ToDoList.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;


namespace ToDoList.Application.Tests.ToDo
{
    public class GetAllTest
    {
        private readonly DataContext _context;
        private readonly GetAll.Handler _handler;

        public GetAllTest()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) 
                .Options;

            _context = new DataContext(options);
            _handler = new GetAll.Handler(_context);
        }

        [Fact]
        public async Task Handle_ShouldReturnListOfToDoItems_WhenDataExists()
        {
            // Arrange
            _context.ToDoList.AddRange(new List<ToDoItem>
            {
                new ToDoItem
                {
                    Id = Guid.NewGuid(),
                    Title = "Test Task 1",
                    PercentCompleted = 50,
                    TimeOfExpiry = DateTime.Now.AddDays(1),
                    Description = "Test 1"
                },
                new ToDoItem
                {
                    Id = Guid.NewGuid(),
                    Title = "Test Task 2",
                    PercentCompleted = 75,
                    TimeOfExpiry = DateTime.Now.AddDays(2)
                }
            });

            await _context.SaveChangesAsync();

            var query = new GetAll.Query();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
            result.Value[0].Title.Should().Be("Test Task 1");
            result.Value[1].Title.Should().Be("Test Task 2");
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoDataExists()
        {
            // Arrange
            var query = new GetAll.Query();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }
    }
}
