namespace Bsm.Api.Features.DncNumbers;

/// <summary>A phone number on the internal do-not-call list.</summary>
public sealed class DncNumber
{
    public int Id { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? State { get; set; }
    public DateTime CreatedOn { get; set; }
}
