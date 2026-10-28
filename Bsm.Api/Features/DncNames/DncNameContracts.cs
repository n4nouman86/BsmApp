using System.ComponentModel.DataAnnotations;

namespace Bsm.Api.Features.DncNames;

/// <summary>Editable DNC name entry.</summary>
public sealed record SaveDncNameRequest
{
    [Required, MaxLength(50)]
    public required string FirstName { get; init; }

    [Required, MaxLength(50)]
    public required string LastName { get; init; }

    /// <summary>Optional US phone number; stored as 10 digits when provided.</summary>
    [MaxLength(50)]
    public string? Phone { get; init; }

    /// <summary>US state abbreviation, for example TX.</summary>
    [MaxLength(50)]
    public string? State { get; init; }
}

/// <summary>A DNC name entry returned by the API.</summary>
public sealed record DncNameResponse(int Id, string FirstName, string LastName, string? Phone, string? State);
