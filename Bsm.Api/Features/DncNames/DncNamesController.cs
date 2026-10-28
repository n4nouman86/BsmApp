using Bsm.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bsm.Api.Features.DncNames;

[Authorize(Roles = "Admin")]
public sealed class DncNamesController(IDncNameService service) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List all DNC names (administrators only).")]
    [ProducesResponseType<IReadOnlyList<DncNameResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DncNameResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [EndpointSummary("Get a DNC name.")]
    [ProducesResponseType<DncNameResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DncNameResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var entry = await service.GetByIdAsync(id, cancellationToken);
        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpPost]
    [EndpointSummary("Add a name to the DNC list.")]
    [ProducesResponseType<DncNameResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DncNameResponse>> Create(SaveDncNameRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return result.Outcome == SaveDncNameOutcome.Saved
            ? CreatedAtAction(nameof(GetById), new { id = result.Entry!.Id }, result.Entry)
            : Problem(result);
    }

    [HttpPut("{id:int}")]
    [EndpointSummary("Update a DNC name.")]
    [ProducesResponseType<DncNameResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DncNameResponse>> Update(int id, SaveDncNameRequest request, CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        return result.Outcome == SaveDncNameOutcome.Saved ? Ok(result.Entry) : Problem(result);
    }

    [HttpDelete("{id:int}")]
    [EndpointSummary("Remove a name from the DNC list.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        await service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();

    private ActionResult Problem(SaveDncNameResult result) => result.Outcome == SaveDncNameOutcome.NotFound
        ? NotFound()
        : BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            [nameof(SaveDncNameRequest.Phone)] = ["Enter a valid 10-digit US phone number."],
        }));
}
