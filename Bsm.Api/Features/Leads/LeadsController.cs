using System.Security.Claims;
using Bsm.Api.Controllers;
using Bsm.Api.Features.Tcpa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bsm.Api.Features.Leads;

[Authorize]
public sealed class LeadsController(ILeadService service, ITcpaService tcpa) : ApiControllerBase
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
        var (phoneError, phone) = await CheckPhoneAsync(request.PhoneNumber, cancellationToken);
        if (phoneError is not null) return phoneError;
        var lead = await service.CreateAsync(request with { PhoneNumber = phone! }, userId, cancellationToken);
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
        var existing = await service.GetByIdAsync(id, userId, User.IsInRole("Admin"), cancellationToken);
        if (existing is null) return NotFound();

        // Only scrub when the number actually changed, so edits to other fields do not spend a TCPA lookup.
        var normalized = tcpa.Normalize(request.PhoneNumber);
        var phone = normalized;
        if (normalized is null || normalized != tcpa.Normalize(existing.PhoneNumber))
        {
            var (phoneError, checkedPhone) = await CheckPhoneAsync(request.PhoneNumber, cancellationToken);
            if (phoneError is not null) return phoneError;
            phone = checkedPhone;
        }

        var lead = await service.UpdateAsync(id, request with { PhoneNumber = phone ?? request.PhoneNumber }, userId, User.IsInRole("Admin"), cancellationToken);
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

    private async Task<(ActionResult? Error, string? Phone)> CheckPhoneAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        var phone = tcpa.Normalize(phoneNumber);
        if (phone is null) return (PhoneProblem("Enter a valid 10-digit US phone number."), null);

        var result = await tcpa.CheckAsync(phone, cancellationToken);
        if (result.Status == "unavailable")
        {
            return (Problem("The TCPA check is unavailable. Please try again.", statusCode: StatusCodes.Status503ServiceUnavailable), null);
        }

        return result.IsClean ? (null, phone) : (PhoneProblem("This number cannot be used because it is on the TCPA litigator list."), null);
    }

    private BadRequestObjectResult PhoneProblem(string message) =>
        BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]> { [nameof(SaveLeadRequest.PhoneNumber)] = [message] }));

    private BadRequestObjectResult InvalidStatus() =>
        BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            [nameof(SaveLeadRequest.StatusId)] = ["The selected status does not exist."],
        }));
}
