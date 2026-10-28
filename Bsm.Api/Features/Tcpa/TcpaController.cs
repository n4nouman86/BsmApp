using Bsm.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bsm.Api.Features.Tcpa;

[Authorize]
public sealed class TcpaController(ITcpaService service) : ApiControllerBase
{
    [HttpGet("check")]
    [EndpointSummary("Scrub a US phone number against the TCPA litigator list.")]
    [ProducesResponseType<TcpaCheckResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<TcpaCheckResponse>> Check([FromQuery] string phoneNumber, CancellationToken cancellationToken)
    {
        var phone = service.Normalize(phoneNumber);
        if (phone is null)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["phoneNumber"] = ["Enter a valid 10-digit US phone number."],
            }));
        }

        var result = await service.ScrubAsync(phone, cancellationToken);
        if (result.Status == "unavailable")
        {
            return Problem("The TCPA check is unavailable. Please try again.", statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        return Ok(new TcpaCheckResponse(phone, result.IsClean, result.Status));
    }

    [HttpGet("dnc-check")]
    [EndpointSummary("Check a US phone number against the internal DNC numbers list.")]
    [ProducesResponseType<DncCheckResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DncCheckResponse>> DncCheck([FromQuery] string phoneNumber, CancellationToken cancellationToken)
    {
        var phone = service.Normalize(phoneNumber);
        if (phone is null)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["phoneNumber"] = ["Enter a valid 10-digit US phone number."],
            }));
        }

        return Ok(new DncCheckResponse(phone, await service.IsOnDncListAsync(phone, cancellationToken)));
    }
}
