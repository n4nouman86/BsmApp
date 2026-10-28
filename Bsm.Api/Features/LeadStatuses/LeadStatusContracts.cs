using System.ComponentModel.DataAnnotations;

namespace Bsm.Api.Features.LeadStatuses;

/// <summary>Editable lead status details. The creation time is assigned by the server.</summary>
public sealed record SaveLeadStatusRequest
{
    /// <summary>Status name, unique ignoring case.</summary>
    [Required, MaxLength(50)]
    public required string Name { get; init; }
}

/// <summary>A lead status returned by the API.</summary>
public sealed record LeadStatusResponse(int Id, string Name, DateTimeOffset CreatedOn);
