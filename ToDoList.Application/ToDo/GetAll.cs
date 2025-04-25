using MediatR;
using Microsoft.EntityFrameworkCore;
using ToDoList.Infrastructure;
using ToDoList.Domain;

namespace ToDoList.Application.ToDo
{
    public class GetAll
    {
        // Query request that asks for a list of ToDo items
        public class Query : IRequest<Result<List<ToDoItem>>> { }

        // Handler that processes the Query
        public class Handler : IRequestHandler<Query, Result<List<ToDoItem>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<ToDoItem>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Fetch all ToDo items from the database
                var result = await _context.ToDoList.ToListAsync();

                // Return the result wrapped in a success response
                return Result<List<ToDoItem>>.Success(result);
            }
        }
    }
}
