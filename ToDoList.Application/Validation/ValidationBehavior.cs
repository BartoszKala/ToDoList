using FluentValidation; 
using MediatR; 

namespace ToDoList.Application.Validation
{
    // This is a generic validation behavior that can be used with any request and response types
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse> 
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators; // List of validators for TRequest

        // Constructor takes a list of validators for the TRequest type
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        // The Handle method is called for each request in the pipeline
        public async Task<TResponse> Handle(
            TRequest request, // The request to be validated
            RequestHandlerDelegate<TResponse> next, // The next handler in the pipeline
            CancellationToken cancellationToken) // for task cancellation
        {
            
            if (_validators.Any())
            {

                var context = new ValidationContext<TRequest>(request);

                // Run all validators asynchronously
                var validationResults = await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken))
                );

                // Collect all validation errors
                var failures = validationResults
                    .SelectMany(r => r.Errors) // Flatten the list of validation errors
                    .Where(f => f != null) // Ensure no null errors are included
                    .ToList();

                if (failures.Count != 0)
                    throw new ValidationException(failures);
            }

            // Proceed to the next step in the pipeline (the actual request handler)
            return await next();
        }
    }
}
