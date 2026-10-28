using System.Security.Claims;
using Bsm.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bsm.Api.Features.Leads;

[Authorize]
public sealed class LeadsController(ILeadService service) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List leads owned by the current user, or all leads for administrators.")]
    [ProducesResponseType<IReadOnlyList<LeadResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<LeadResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        return Ok(await service.GetAllAsync(userId, User.IsInRole("Admin"), cancellationToken));
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("Get a lead accessible to the current user.")]
    [ProducesResponseType<LeadResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LeadResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        var lead = await service.GetByIdAsync(id, userId, User.IsInRole("Admin"), cancellationToken);
        return lead is null ? NotFound() : Ok(lead);
    }

    [HttpPost]
    [EndpointSummary("Create a lead owned by the current user.")]
    [ProducesResponseType<LeadResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LeadResponse>> Create(SaveLeadRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        if (!await service.StatusExistsAsync(request.StatusId!.Value, cancellationToken)) return InvalidStatus();
        var lead = await service.CreateAsync(request, userId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = lead.Id }, lead);
    }

    [HttpPut("{id:int}")]
    [EndpointSummary("Update an accessible lead without changing its owner or creation time.")]
    [ProducesResponseType<LeadResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LeadResponse>> Update(int id, SaveLeadRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        if (!await service.StatusExistsAsync(request.StatusId!.Value, cancellationToken)) return InvalidStatus();
        var lead = await service.UpdateAsync(id, request, userId, User.IsInRole("Admin"), cancellationToken);
        return lead is null ? NotFound() : Ok(lead);
    }

    [HttpDelete("{id:int}")]
    [EndpointSummary("Permanently delete an accessible lead.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        return await service.DeleteAsync(id, userId, User.IsInRole("Admin"), cancellationToken)
            ? NoContent() : NotFound();
    }

    private BadRequestObjectResult InvalidStatus() =>
        BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            [nameof(SaveLeadRequest.StatusId)] = ["The selected status does not exist."],
        }));
}
