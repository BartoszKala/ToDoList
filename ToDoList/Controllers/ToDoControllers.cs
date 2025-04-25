using Microsoft.AspNetCore.Mvc;
using static ToDoList.Controllers.BaseController;
using ToDoList.Domain;
using ToDoList.Application.ToDo;

namespace ToDoList.Controllers
{
    // [Authorize] // Uncomment to secure the controller with authorization
    public class ToDo : BaseApiController
    {
        // GET: api/todo
        // Retrieves all ToDo items
        [HttpGet]
        public async Task<ActionResult<List<ToDoItem>>> GetAll()
        {
            var result = await Mediator.Send(new GetAll.Query());

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return result.Value != null ? Ok(result.Value) : NotFound();
        }

        // GET: api/todo/incoming?filter=Today
        // Retrieves upcoming ToDo items based on a date filter
        [HttpGet("incoming")]
        public async Task<ActionResult<List<ToDoItem>>> GetIncoming([FromQuery] IncommingToDo.DateFilter filter = IncommingToDo.DateFilter.None)
        {
            var result = await Mediator.Send(new IncommingToDo.Query { Filter = filter });

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return result.Value != null ? Ok(result.Value) : NotFound();
        }

        // GET: api/todo/{id}
        // Retrieves a specific ToDo item by its ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSpecific(Guid id)
        {
            var result = await Mediator.Send(new SpecificToDo.Query { Id = id });

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return result.Value != null ? Ok(result.Value) : NotFound();
        }

        // POST: api/todo
        // Creates a new ToDo item
        [HttpPost]
        public async Task<IActionResult> CreateToDo(ToDoItem toDo)
        {
            var result = await Mediator.Send(new Create.Command { toDo = toDo });

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        // PUT: api/todo/{id}
        // Updates an existing ToDo item
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateToDo(Guid id, ToDoItem toDo)
        {
            toDo.Id = id;
            var result = await Mediator.Send(new Update.Command { toDo = toDo });

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        // PATCH: api/todo/{id}/{percent}
        // Updates the percentage of completion for a ToDo item
        [HttpPatch("{id}/{percent}")]
        public async Task<IActionResult> UpdatePercent(Guid id, int percent)
        {
            var command = new UpdatePercent.Command
            {
                Id = id,
                PercentCompleted = percent
            };

            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok();
        }

        // PATCH: api/todo/{id}/done
        // Marks the ToDo item as done (100% complete)
        [HttpPatch("{id}/done")]
        public async Task<IActionResult> SetAsDone(Guid id)
        {
            var command = new UpdatePercent.Command
            {
                Id = id,
                PercentCompleted = 100
            };

            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok();
        }

        // DELETE: api/todo/{id}
        // Deletes a ToDo item by ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteToDo(Guid id)
        {
            var result = await Mediator.Send(new Delete.Command { Id = id });

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok();
        }
    }
}
