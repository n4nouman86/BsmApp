namespace Bsm.Api.Features.DncNames;

/// <summary>A person's name (optionally with phone) on the internal do-not-call list.</summary>
public sealed class DncName
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? State { get; set; }
}
