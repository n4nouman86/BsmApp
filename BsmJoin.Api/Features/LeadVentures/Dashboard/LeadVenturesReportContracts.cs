namespace BsmJoin.Api.Features.LeadVentures.Dashboard;

/// <summary>Daily transfer counts from the LeadVentures reporting view.</summary>
public sealed record LeadVenturesTransferResponse(
    DateOnly? TransferDate,
    int? Total,
    int? Billable,
    int? NonBillable);

/// <summary>Agent performance returned by the existing LeadVentures report procedures.</summary>
public sealed record LeadVenturesReportResponse(
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
