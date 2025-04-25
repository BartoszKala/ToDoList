using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ToDoList.Application.Validation
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next; // Next middleware in the pipeline
        private readonly ILogger<ExceptionMiddleware> _logger; // Logger to log exceptions

        // Constructor to inject dependencies (next middleware and logger)
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // The method invoked on every request
        public async Task Invoke(HttpContext context)
        {
            try
            {
                // Call the next middleware in the pipeline
                await _next(context);
            }
            catch (ValidationException ex) 
            {
                // Set the HTTP status code to BadRequest (400) for validation errors
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";

                // Map validation errors to a simpler format and send them as a JSON response
                var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });

                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    message = "Validation failed", 
                    errors // List of validation errors
                }));
            }
            catch (Exception ex) 
            {
                // Log the exception for debugging or monitoring purposes
                _logger.LogError(ex, "Unhandled exception");

                // Set the HTTP status code to InternalServerError (500) for unhandled exceptions
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                // Send a generic error message as JSON
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    message = "Something went wrong"
                }));
            }
        }
    }
}
