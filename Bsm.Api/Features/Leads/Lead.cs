namespace Bsm.Api.Features.Leads;

public sealed class Lead
{
    public int Id { get; set; }
    public string? AppUserId { get; set; }
    public required string PhoneNumber { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public required string StreetAddress { get; set; }
    public string? City { get; set; }
    public required string State { get; set; }
    public string? PostalCode { get; set; }
    public required string HaveSpouse { get; set; }
    public string? SpouseName { get; set; }
    public DateOnly? SpouseDateOfBirth { get; set; }
    public required string CurrentAutoCarrier { get; set; }
    public required string AnyLaspse { get; set; }
    public required string MakeAndModel { get; set; }
    public required string Accidents { get; set; }
    public required string Tikets { get; set; }
    public required string HomeOwner { get; set; }
    public string? HopeType { get; set; }
    public string? HaveHomeClaims { get; set; }
    public string? CurrentHomeCarrier { get; set; }
    public string? AnyClaimInPast3Years { get; set; }
    public DateTime CreatedOn { get; set; }
}
