using MediatR;
using ToDoList.Domain;
using ToDoList.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ToDoList.Application.ToDo
{
    public class IncommingToDo
    {
        // 1) Enum to define filter options based on due date
        public enum DateFilter
        {
            None,
            Today,
            Tomorrow,
            ThisWeek,
            ThisMonth
        }

        // 2) Query object containing the selected filter
        public class Query : IRequest<Result<List<ToDoItem>>>
        {
            public DateFilter Filter { get; init; } = DateFilter.None;
        }

        // 3) Handler that processes the query and applies filtering logic
        public class Handler : IRequestHandler<Query, Result<List<ToDoItem>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<ToDoItem>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var today = DateTime.UtcNow.Date;
                IQueryable<ToDoItem> query = _context.ToDoList;

                // Apply date-based filter according to the request
                switch (request.Filter)
                {
                    case DateFilter.Today:
                        query = query.Where(x => x.TimeOfExpiry.Date == today);
                        break;
                    case DateFilter.Tomorrow:
                        var tomorrow = today.AddDays(1);
                        query = query.Where(x => x.TimeOfExpiry.Date == tomorrow);
                        break;
                    case DateFilter.ThisWeek:
                        var weekEnd = today.AddDays(7);
                        query = query.Where(x => x.TimeOfExpiry.Date >= today && x.TimeOfExpiry.Date <= weekEnd);
                        break;
                    case DateFilter.ThisMonth:
                        var monthEnd = today.AddMonths(1);
                        query = query.Where(x => x.TimeOfExpiry.Date >= today && x.TimeOfExpiry.Date <= monthEnd);
                        break;
                }

                // Execute the query and return the result
                var list = await query.ToListAsync(cancellationToken);
                return Result<List<ToDoItem>>.Success(list);
            }
        }
    }
}
