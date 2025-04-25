using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ToDoList.Application.ToDo;
using ToDoList.Infrastructure;
using ToDoList.Domain;

namespace ToDoList.Application.Tests.ToDo
{
    public class IncommingToDoTest
    {
        private readonly DataContext _context;
        private readonly IncommingToDo.Handler _handler;

        public IncommingToDoTest()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DataContext(options);
            _handler = new IncommingToDo.Handler(_context);
        }

        private async Task SeedDataAsync()
        {
            var today = DateTime.UtcNow.Date;

            var items = new List<ToDoItem>
            {
                new ToDoItem { Id = Guid.NewGuid(), Title = "Today Task", TimeOfExpiry = today, PercentCompleted = 0 },
                new ToDoItem { Id = Guid.NewGuid(), Title = "Tomorrow Task", TimeOfExpiry = today.AddDays(1), PercentCompleted = 0 },
                new ToDoItem { Id = Guid.NewGuid(), Title = "Next Week Task", TimeOfExpiry = today.AddDays(5), PercentCompleted = 0 },
                new ToDoItem { Id = Guid.NewGuid(), Title = "Next Month Task", TimeOfExpiry = today.AddDays(25), PercentCompleted = 0 },
                new ToDoItem { Id = Guid.NewGuid(), Title = "Far Future Task", TimeOfExpiry = today.AddMonths(2), PercentCompleted = 0 },
            };

            _context.ToDoList.AddRange(items);
            await _context.SaveChangesAsync();
        }

        [Theory]
        [InlineData(IncommingToDo.DateFilter.Today, 1)]
        [InlineData(IncommingToDo.DateFilter.Tomorrow, 1)]
        [InlineData(IncommingToDo.DateFilter.ThisWeek, 3)] // today, tomorrow, next week
        [InlineData(IncommingToDo.DateFilter.ThisMonth, 4)] // all except Far Future
        [InlineData(IncommingToDo.DateFilter.None, 5)] // all
        public async Task Handle_ShouldReturnFilteredResults_BasedOnDateFilter(IncommingToDo.DateFilter filter, int expectedCount)
        {
            // Arrange
            await SeedDataAsync();

            var query = new IncommingToDo.Query { Filter = filter };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(expectedCount);
        }
    }
}
