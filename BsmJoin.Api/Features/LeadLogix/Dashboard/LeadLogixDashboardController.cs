using System.ComponentModel.DataAnnotations;
using BsmJoin.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BsmJoin.Api.Features.LeadLogix.Dashboard;

[Authorize(Roles = "Admin")]
[Route("api/leadlogix/dashboard")]
public sealed class LeadLogixDashboardController(ILeadLogixReportService reports) : ApiControllerBase
{
    [HttpGet("transfers")]
    [EndpointSummary("Get the latest seven recorded LeadLogix transfer days.")]
    [ProducesResponseType<IReadOnlyList<LeadLogixTransferResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IReadOnlyList<LeadLogixTransferResponse>>> GetTransfers(CancellationToken cancellationToken)
        => Ok(await reports.GetTransfersAsync(cancellationToken));

    [HttpGet("daily")]
    [EndpointSummary("Get the LeadLogix agent day report for the selected date.")]
    [ProducesResponseType<IReadOnlyList<LeadLogixReportResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IReadOnlyList<LeadLogixReportResponse>>> GetDaily(
        [FromQuery, Required] DateOnly? date, CancellationToken cancellationToken)
        => Ok(await reports.GetDailyAsync(date!.Value, cancellationToken));

    [HttpGet("weekly")]
    [EndpointSummary("Get the LeadLogix agent report for an inclusive date range.")]
    [ProducesResponseType<IReadOnlyList<LeadLogixReportResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IReadOnlyList<LeadLogixReportResponse>>> GetWeekly(
        [FromQuery, Required] DateOnly? from,
        [FromQuery, Required] DateOnly? to,
        CancellationToken cancellationToken)
    {
        if (from > to)
        {
            ModelState.AddModelError(nameof(to), "The end date must be on or after the start date.");
            return ValidationProblem(ModelState);
        }

        return Ok(await reports.GetWeeklyAsync(from!.Value, to!.Value, cancellationToken));
    }
}
