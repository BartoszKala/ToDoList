using ToDoList.Infrastructure;
using ToDoList.Domain;
using FluentValidation;
using MediatR;
using ToDoList.Application.Validation;

namespace ToDoList.Application.ToDo
{
    // Represents a "command" to create a new ToDo item
    public class Create
    {
        // Command object sent via MediatR, containing the ToDo item to create
        public class Command : IRequest<Result<Unit>>
        {
            public required ToDoItem toDo { get; set; }
        }

        // FluentValidation rules for the Command
        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.toDo).SetValidator(new ToDoValidator());
            }
        }

        // Handler that processes the command
        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                // Add the ToDo item to the database context
                _context.ToDoList.Add(request.toDo);

                var result = await _context.SaveChangesAsync() > 0;

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
