using FluentValidation;
using MediatR;
using ToDoList.Infrastructure;

namespace ToDoList.Application.ToDo
{
    public class UpdatePercent
    {
        // Command to update the percentage completion of a ToDo item or  set as done
        public class Command : IRequest<Result<Unit>>
        {
            public required Guid Id { get; init; } 
            public required int PercentCompleted { get; init; } 
        }

        // Validator to ensure that the PercentCompleted value is between 0 and 100
        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.PercentCompleted)
                    .InclusiveBetween(0, 100) 
                    .WithMessage("Percent must be between 0 and 100");
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
                // 1. Retrieve the existing ToDo item from the database using its ID
                var toDoItem = await _context.ToDoList.FindAsync(new object[] { request.Id }, cancellationToken);
                if (toDoItem == null)
                    return Result<Unit>.Failure("ToDoItem not found.");

                // 2. If the percentage hasn't changed, return success without making any changes
                if (toDoItem.PercentCompleted == request.PercentCompleted)
                    return Result<Unit>.Success(Unit.Value);

                // 3. Update only the PercentCompleted field
                toDoItem.PercentCompleted = request.PercentCompleted;

                // 4. Save the changes to the database
                var updated = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!updated)
                    return Result<Unit>.Failure("Failed to update percent.");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
