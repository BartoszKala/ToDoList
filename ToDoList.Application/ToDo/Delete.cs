using MediatR;
using ToDoList.Infrastructure;

namespace ToDoList.Application.ToDo
{
    public class Delete
    {
        // Command that carries the ID of the ToDo item to delete
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
        }

        // Handler that processes the delete command
        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                // Try to find the ToDo item by its ID
                var toDoItem = await _context.ToDoList.FindAsync(request.Id);

                // If not found, optionally return a failure here
                if (toDoItem == null)
                    return Result<Unit>.Failure("ToDo item not found");

                _context.Remove(toDoItem);

                // Persist the changes to the database
                var result = await _context.SaveChangesAsync() > 0;

                // Return failure if the delete operation didn't save successfully
                if (!result) return Result<Unit>.Failure("Failed to delete the ToDo");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
