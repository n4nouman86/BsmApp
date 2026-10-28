using System.ComponentModel.DataAnnotations;

namespace Bsm.Api.Features.DncNumbers;

/// <summary>Editable DNC entry. The creation time is assigned by the server.</summary>
public sealed record SaveDncNumberRequest
{
    /// <summary>US phone number; stored as 10 digits and unique.</summary>
    [Required, MaxLength(50)]
    public required string Phone { get; init; }

    [MaxLength(50)]
    public string? FirstName { get; init; }

    [MaxLength(50)]
    public string? LastName { get; init; }

    /// <summary>US state abbreviation, for example TX.</summary>
    [MaxLength(50)]
    public string? State { get; init; }
}

/// <summary>A DNC entry returned by the API.</summary>
public sealed record DncNumberResponse(int Id, string Phone, string? FirstName, string? LastName, string? State, DateTimeOffset CreatedOn);
