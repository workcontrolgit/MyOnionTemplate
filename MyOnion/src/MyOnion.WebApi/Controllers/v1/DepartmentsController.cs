namespace MyOnion.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    public class DepartmentsController : BaseApiController
    {
        /// <summary>
        /// Gets a list of departments based on the specified filter.
        /// </summary>
        /// <param name="filter">The filter used to get the list of departments.</param>
        /// <returns>A list of departments.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get([FromQuery] GetDepartmentsQuery filter)
        {
            return Ok(await Mediator.Send(filter));
        }

        /// <summary>
        /// Gets a single department by identifier.
        /// </summary>
        /// <param name="id">Department identifier.</param>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await Mediator.Send(new GetDepartmentByIdQuery { Id = id }));
        }

        /// <summary>
        /// Creates a new department.
        /// </summary>
        /// <param name="command">Department payload.</param>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(CreateDepartmentCommand command)
        {
            var result = await Mediator.Send(command);
            return CreatedAtAction(nameof(Get), new { id = result.Value }, result);
        }

        /// <summary>
        /// Updates the specified department.
        /// </summary>
        /// <param name="id">Department identifier.</param>
        /// <param name="command">Update payload.</param>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(Guid id, UpdateDepartmentCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            return Ok(await Mediator.Send(command));
        }

        /// <summary>
        /// Deletes the specified department.
        /// </summary>
        /// <param name="id">Department identifier.</param>
        [HttpDelete("{id}")]
        [Authorize(Policy = AuthorizationConsts.AdminPolicy)]
        public async Task<IActionResult> Delete(Guid id)
        {
            return Ok(await Mediator.Send(new DeleteDepartmentByIdCommand { Id = id }));
        }
    }
}
