namespace BsmJoin.Api.Features.LeadLogix.Dashboard;

/// <summary>Daily transfer counts from the LeadLogix reporting view.</summary>
public sealed record LeadLogixTransferResponse(
    DateOnly? TransferDate,
    int? Total,
    int? Billable,
    int? NonBillable);

/// <summary>Agent performance returned by the existing LeadLogix report procedures.</summary>
public sealed record LeadLogixReportResponse(
    DateOnly? TransferDate,
    string FullName,
    string States,
    DateTimeOffset? ScheduleFrom,
    DateTimeOffset? ScheduleTo,
    int Target,
    int Achieved,
    int Billable,
    int NonBillable,
    decimal? CostPerLead,
    decimal TotalAmount);
