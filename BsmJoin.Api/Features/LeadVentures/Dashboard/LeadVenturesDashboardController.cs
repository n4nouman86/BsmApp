using System.ComponentModel.DataAnnotations;
using BsmJoin.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BsmJoin.Api.Features.LeadVentures.Dashboard;

[Authorize(Roles = "Admin")]
[Route("api/leadventures/dashboard")]
public sealed class LeadVenturesDashboardController(ILeadVenturesReportService reports) : ApiControllerBase
{
    [HttpGet("transfers")]
    [EndpointSummary("Get the latest seven recorded LeadVentures transfer days.")]
    [ProducesResponseType<IReadOnlyList<LeadVenturesTransferResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IReadOnlyList<LeadVenturesTransferResponse>>> GetTransfers(CancellationToken cancellationToken)
        => Ok(await reports.GetTransfersAsync(cancellationToken));

    [HttpGet("daily")]
    [EndpointSummary("Get the LeadVentures agent day report for the selected date.")]
    [ProducesResponseType<IReadOnlyList<LeadVenturesReportResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IReadOnlyList<LeadVenturesReportResponse>>> GetDaily(
        [FromQuery, Required] DateOnly? date, CancellationToken cancellationToken)
        => Ok(await reports.GetDailyAsync(date!.Value, cancellationToken));

    [HttpGet("weekly")]
    [EndpointSummary("Get the LeadVentures agent report for an inclusive date range.")]
    [ProducesResponseType<IReadOnlyList<LeadVenturesReportResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IReadOnlyList<LeadVenturesReportResponse>>> GetWeekly(
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
