using MediatR;
using ToDoList.Domain;
using ToDoList.Infrastructure;

namespace ToDoList.Application.ToDo
{
    public class SpecificToDo
    {
        // Query to fetch a specific ToDo item by its ID
        public class Query : IRequest<Result<ToDoItem>>
        {
            public Guid Id { get; set; }
        }

        // Handler to process the query
        public class Handler : IRequestHandler<Query, Result<ToDoItem>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<ToDoItem>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Try to find the ToDo item in the database using its ID
                var item = await _context.ToDoList.FindAsync(request.Id);

                if (item == null)
                {
                    // Return a failure result if the item was not found
                    return Result<ToDoItem>.Failure("ToDo item not found.");
                }

                return Result<ToDoItem>.Success(item);
            }
        }

    }
}
