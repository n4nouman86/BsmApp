using System.ComponentModel.DataAnnotations;

namespace Bsm.Api.Features.Leads;

/// <summary>Editable lead details. Ownership and creation time are assigned by the server.</summary>
public sealed record SaveLeadRequest
{
    [Required, MaxLength(50)]
    public required string PhoneNumber { get; init; }
    [Required, MaxLength(50)]
    public required string FirstName { get; init; }
    [Required, MaxLength(50)]
    public required string LastName { get; init; }
    [Required, EmailAddress, MaxLength(50)]
    public required string Email { get; init; }
    [Required]
    public required DateOnly? DateOfBirth { get; init; }
    [Required, MaxLength(256)]
    public required string StreetAddress { get; init; }
    [MaxLength(50)]
    public string? City { get; init; }
    [Required, MaxLength(50)]
    public required string State { get; init; }
    [MaxLength(50)]
    public string? PostalCode { get; init; }
    [Required, MaxLength(50)]
    public required string HaveSpouse { get; init; }
    [MaxLength(50)]
    public string? SpouseName { get; init; }
    public DateOnly? SpouseDateOfBirth { get; init; }
    [Required, MaxLength(50)]
    public required string CurrentAutoCarrier { get; init; }
    [Required, MaxLength(50)]
    public required string AnyLaspse { get; init; }
    [Required, MaxLength(1024)]
    public required string MakeAndModel { get; init; }
    [Required, MaxLength(50)]
    public required string Accidents { get; init; }
    [Required, MaxLength(50)]
    public required string Tikets { get; init; }
    [Required, MaxLength(50)]
    public required string HomeOwner { get; init; }
    [MaxLength(50)]
    public string? HopeType { get; init; }
    [MaxLength(50)]
    public string? HaveHomeClaims { get; init; }
    [MaxLength(50)]
    public string? CurrentHomeCarrier { get; init; }
    [MaxLength(50)]
    public string? AnyClaimInPast3Years { get; init; }
    [Required, Range(1, int.MaxValue)]
    public required int? StatusId { get; init; }
}

/// <summary>A lead visible to its owner or an administrator. Birth dates are calendar dates without a time zone.</summary>
public sealed record LeadResponse(
    int Id,
    string? AppUserId,
    string PhoneNumber,
    string FirstName,
    string LastName,
    string Email,
    DateOnly DateOfBirth,
    string StreetAddress,
    string? City,
    string State,
    string? PostalCode,
    string HaveSpouse,
    string? SpouseName,
    DateOnly? SpouseDateOfBirth,
    string CurrentAutoCarrier,
    string AnyLaspse,
    string MakeAndModel,
    string Accidents,
    string Tikets,
    string HomeOwner,
    string? HopeType,
    string? HaveHomeClaims,
    string? CurrentHomeCarrier,
    string? AnyClaimInPast3Years,
    int StatusId,
    string StatusName,
    DateTimeOffset CreatedOn);
