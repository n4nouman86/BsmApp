using Bsm.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bsm.Api.Features.LeadStatuses;

[Authorize]
public sealed class LeadStatusesController(ILeadStatusService service) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List all lead statuses.")]
    [ProducesResponseType<IReadOnlyList<LeadStatusResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LeadStatusResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [EndpointSummary("Get a lead status.")]
    [ProducesResponseType<LeadStatusResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeadStatusResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var status = await service.GetByIdAsync(id, cancellationToken);
        return status is null ? NotFound() : Ok(status);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Create a lead status (administrators only).")]
    [ProducesResponseType<LeadStatusResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LeadStatusResponse>> Create(SaveLeadStatusRequest request, CancellationToken cancellationToken)
    {
        var status = await service.CreateAsync(request, cancellationToken);
        return status is null
            ? Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "A status with this name already exists." })
            : CreatedAtAction(nameof(GetById), new { id = status.Id }, status);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Rename a lead status (administrators only).")]
    [ProducesResponseType<LeadStatusResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LeadStatusResponse>> Update(int id, SaveLeadStatusRequest request, CancellationToken cancellationToken)
    {
        switch (await service.UpdateAsync(id, request, cancellationToken))
        {
            case UpdateStatusResult.NotFound:
                return NotFound();
            case UpdateStatusResult.Duplicate:
                return Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "A status with this name already exists." });
            default:
                return Ok(await service.GetByIdAsync(id, cancellationToken));
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Permanently delete a lead status (administrators only).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        await service.DeleteAsync(id, cancellationToken) switch
        {
            DeleteStatusResult.Deleted => NoContent(),
            DeleteStatusResult.InUse => Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "This status is used by existing leads and cannot be deleted." }),
            _ => NotFound(),
        };
}
