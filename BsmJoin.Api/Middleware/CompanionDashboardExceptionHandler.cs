using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BsmJoin.Api.Middleware;

internal sealed class CompanionDashboardExceptionHandler(ILogger<CompanionDashboardExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var path = httpContext.Request.Path;
        if ((!path.StartsWithSegments("/api/leadlogix/dashboard")
                && !path.StartsWithSegments("/api/leadventures/dashboard"))
            || exception is not SqlException)
        {
            return false;
        }

        logger.LogError(exception, "Companion-app report database query failed.");
        httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status503ServiceUnavailable,
            Title = "Companion-app reports are unavailable.",
            Detail = "Unable to read the app database. Check the server connection and try again.",
        }, cancellationToken);
        return true;
    }
}
