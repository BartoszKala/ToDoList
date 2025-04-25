using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ToDoList.Application.Validation;
using FluentValidation;
using System.Text.Json;
using System.Net;
using FluentAssertions;

namespace ToDoList.Tests.Middleware
{
    public class ExceptionMiddlewareTests
    {
        private readonly Mock<ILogger<ExceptionMiddleware>> _loggerMock;
        private readonly ExceptionMiddleware _middleware;

        public ExceptionMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
            _middleware = new ExceptionMiddleware(
                async (innerHttpContext) => await Task.CompletedTask,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task Invoke_ShouldReturnBadRequest_WhenValidationExceptionIsThrown()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var validationException = new ValidationException("Validation failed", new[]
            {
                new FluentValidation.Results.ValidationFailure("Property1", "Error1"),
                new FluentValidation.Results.ValidationFailure("Property2", "Error2")
            });

            var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            var exceptionMiddleware = new ExceptionMiddleware(
                async (innerHttpContext) => throw validationException,
                _loggerMock.Object
            );

            // Act
            await exceptionMiddleware.Invoke(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

            errorResponse.GetProperty("message").GetString().Should().Be("Validation failed");
            errorResponse.GetProperty("errors").GetArrayLength().Should().Be(2); 
        }

        [Fact]
        public async Task Invoke_ShouldReturnInternalServerError_WhenUnhandledExceptionIsThrown()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var unhandledException = new Exception("An unexpected error occurred");

            var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            var exceptionMiddleware = new ExceptionMiddleware(
                async (innerHttpContext) => throw unhandledException,
                _loggerMock.Object
            );

            // Act
            await exceptionMiddleware.Invoke(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

            errorResponse.GetProperty("message").GetString().Should().Be("Something went wrong");
        }

        [Fact]
        public async Task Invoke_ShouldCallNextMiddleware_WhenNoExceptionIsThrown()
        {
            // Arrange
            var context = new DefaultHttpContext();

            var nextMiddlewareCalled = false;
            var exceptionMiddleware = new ExceptionMiddleware(
                async (innerHttpContext) =>
                {
                    nextMiddlewareCalled = true;
                    await Task.CompletedTask;
                },
                _loggerMock.Object
            );

            // Act
            await exceptionMiddleware.Invoke(context);

            // Assert
            nextMiddlewareCalled.Should().BeTrue();
        }
    }
}
