using ToDoList.Domain;
using FluentValidation;
using MediatR;
using ToDoList.Infrastructure;
using ToDoList.Application.Validation;

namespace ToDoList.Application.ToDo
{
    public class Update
    {
        // Command to update an existing ToDo item
        public class Command : IRequest<Result<Unit>>
        {
            public required ToDoItem toDo { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                // Validate ToDoItem using its own validator 
                RuleFor(x => x.toDo).SetValidator(new ToDoValidator());
            }
        }

        // Handler to process the update command
        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                // 1. Retrieve the existing ToDo item from the database by its ID
                var toDoItem = await _context.ToDoList.FindAsync(request.toDo.Id);
                if (toDoItem == null)
                    return Result<Unit>.Failure("ToDo item not found");

                // 2. Check if any data has actually changed. If not, return success without saving
                if (toDoItem.TimeOfExpiry == request.toDo.TimeOfExpiry &&
                    toDoItem.Title == request.toDo.Title &&
                    toDoItem.Description == request.toDo.Description &&
                    toDoItem.PercentCompleted == request.toDo.PercentCompleted)
                {
                    return Result<Unit>.Success(Unit.Value);
                }

                // 3. Update the fields with the new data, if provided
                toDoItem.TimeOfExpiry = request.toDo.TimeOfExpiry;
                toDoItem.Title = request.toDo.Title ?? toDoItem.Title; // Keep old value if no changes
                toDoItem.Description = request.toDo.Description ?? toDoItem.Description; 
                toDoItem.PercentCompleted = request.toDo.PercentCompleted;

                // 4. Save changes to the database
                var success = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!success)
                    return Result<Unit>.Failure("Failed to update ToDoItem");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
