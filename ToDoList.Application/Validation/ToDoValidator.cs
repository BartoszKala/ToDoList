using FluentValidation; 
using ToDoList.Domain; 

namespace ToDoList.Application.Validation
{
    // Validator class to validate the ToDoItem object
    public class ToDoValidator : AbstractValidator<ToDoItem>
    {
        public ToDoValidator()
        {
 
            RuleFor(x => x.PercentCompleted)
                .InclusiveBetween(0, 100) 
                .WithMessage("Percent must be between 0 and 100");

            // Rule for Title: Title cannot be empty
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("You have to add the title")
                .MaximumLength(100)
                .WithMessage("Title cannot be longer than 100 characters");

            // Rule for TimeOfExpiry: Date cannot be empty
            RuleFor(x => x.TimeOfExpiry)
                .NotEmpty() 
                .WithMessage("Date cannot be empty"); 
        }
    }
}
