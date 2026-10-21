using BlazorPortal.Entities;
using BlazorPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace BsmJoin.Api.Features.LeadLogix.Dashboard;

public interface ILeadLogixReportService
{
    Task<IReadOnlyList<LeadLogixTransferResponse>> GetTransfersAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<LeadLogixReportResponse>> GetDailyAsync(DateOnly date, CancellationToken cancellationToken);
    Task<IReadOnlyList<LeadLogixReportResponse>> GetWeeklyAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken);
}

public sealed class LeadLogixReportService(LeadLogixReportDbContext db) : ILeadLogixReportService
{
    public async Task<IReadOnlyList<LeadLogixTransferResponse>> GetTransfersAsync(CancellationToken cancellationToken)
    {
        var rows = await db.Database.SqlQuery<vwcTotalDayTransfer>(
            $"SELECT TOP (7) TransferDate, Total, BillAble, NonBillAble FROM dbo.vwcTotalDayTransfers ORDER BY TransferDate DESC")
            .ToListAsync(cancellationToken);

        return rows.OrderBy(row => row.TransferDate).Select(row => new LeadLogixTransferResponse(
            row.TransferDate is { } date ? DateOnly.FromDateTime(date) : null,
            row.Total, row.BillAble, row.NonBillAble)).ToArray();
    }

    public async Task<IReadOnlyList<LeadLogixReportResponse>> GetDailyAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var rows = await db.Database.SqlQuery<spTransferDailyReport>(
            $"EXEC dbo.spTransferDailyReport {date.ToDateTime(TimeOnly.MinValue)}")
            .ToListAsync(cancellationToken);

        return rows.Select(row => new LeadLogixReportResponse(
            row.TransferDate, row.FullName, row.States, row.ScheduleFrom, row.ScheduleTo,
            row.Target, row.Achieved, row.Billable, row.NonBillable, row.CostPerLead, row.TotalAmount)).ToArray();
    }

    public async Task<IReadOnlyList<LeadLogixReportResponse>> GetWeeklyAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        var rows = await db.Database.SqlQuery<SpTransferWeeklyReport>(
            $"EXEC dbo.spGetTransferWeeklyReport {from}, {to}")
            .ToListAsync(cancellationToken);

        return rows.Select(row => new LeadLogixReportResponse(
            null, row.FullName, row.States, null, null,
            row.Target, row.Achieved, row.Billable, row.NonBillable, row.CostPerLead, row.TotalAmount)).ToArray();
    }
}
