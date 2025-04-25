using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace ToDoList.Controllers
{
    public class BaseController
    {
        [ApiController]
        [Route("api/[controller]")] // All endpoints will start with "api/{controller-name}"
        public class BaseApiController : ControllerBase
        {
            private IMediator? _mediator;

            // Provides access to the Mediator instance (CQRS pattern)
            // If _mediator is null, it will resolve it from the request's services (DI container)
            protected IMediator Mediator => _mediator ??=
                HttpContext.RequestServices.GetService<IMediator>() ??
                throw new InvalidOperationException("Mediator is not registered in the DI container");
        }
    }
}
