namespace Bsm.Api.Features.LeadStatuses;

public sealed class LeadStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
}
