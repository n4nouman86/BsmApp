using Bsm.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bsm.Api.Features.DncNumbers;

[Authorize(Roles = "Admin")]
public sealed class DncNumbersController(IDncNumberService service) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List all DNC phone numbers (administrators only).")]
    [ProducesResponseType<IReadOnlyList<DncNumberResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DncNumberResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [EndpointSummary("Get a DNC phone number.")]
    [ProducesResponseType<DncNumberResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DncNumberResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var entry = await service.GetByIdAsync(id, cancellationToken);
        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpPost]
    [EndpointSummary("Add a phone number to the DNC list.")]
    [ProducesResponseType<DncNumberResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DncNumberResponse>> Create(SaveDncNumberRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return result.Outcome == SaveDncOutcome.Saved
            ? CreatedAtAction(nameof(GetById), new { id = result.Entry!.Id }, result.Entry)
            : Problem(result);
    }

    [HttpPut("{id:int}")]
    [EndpointSummary("Update a DNC phone number.")]
    [ProducesResponseType<DncNumberResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DncNumberResponse>> Update(int id, SaveDncNumberRequest request, CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        return result.Outcome == SaveDncOutcome.Saved ? Ok(result.Entry) : Problem(result);
    }

    [HttpDelete("{id:int}")]
    [EndpointSummary("Remove a phone number from the DNC list.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        await service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();

    private ActionResult Problem(SaveDncResult result) => result.Outcome switch
    {
        SaveDncOutcome.NotFound => NotFound(),
        SaveDncOutcome.InvalidPhone => BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            [nameof(SaveDncNumberRequest.Phone)] = ["Enter a valid 10-digit US phone number."],
        })),
        _ => Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "This phone number is already on the DNC list." }),
    };
}
