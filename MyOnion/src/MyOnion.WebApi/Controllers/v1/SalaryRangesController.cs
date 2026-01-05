namespace MyOnion.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    public class SalaryRangesController : BaseApiController
    {
        /// <summary>
        /// Gets a list of salary ranges based on the specified filter.
        /// </summary>
        /// <param name="filter">The filter used to get the list of salary ranges.</param>
        /// <returns>A list of salary ranges.</returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetSalaryRangesQuery filter)
        {
            return Ok(await Mediator.Send(filter));
        }

        /// <summary>
        /// Gets a salary range by identifier.
        /// </summary>
        /// <param name="id">Salary range identifier.</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await Mediator.Send(new GetSalaryRangeByIdQuery { Id = id }));
        }

        /// <summary>
        /// Creates a new salary range.
        /// </summary>
        /// <param name="command">Salary range payload.</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(CreateSalaryRangeCommand command)
        {
            var result = await Mediator.Send(command);
            return CreatedAtAction(nameof(Get), new { id = result.Value }, result);
        }

        /// <summary>
        /// Updates an existing salary range.
        /// </summary>
        /// <param name="id">Salary range identifier.</param>
        /// <param name="command">Update payload.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, UpdateSalaryRangeCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            return Ok(await Mediator.Send(command));
        }

        /// <summary>
        /// Deletes a salary range.
        /// </summary>
        /// <param name="id">Salary range identifier.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return Ok(await Mediator.Send(new DeleteSalaryRangeByIdCommand { Id = id }));
        }
    }
}
