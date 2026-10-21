using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorPortal.Models;

public class spTransferDailyReport
{
    public DateOnly TransferDate { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string States { get; set; } = string.Empty;
	public DateTimeOffset? ScheduleFrom { get; set; }
    public DateTimeOffset? ScheduleTo { get; set; }
    public int Target { get; set; }
    public int Achieved { get; set; }
    public int Billable { get; set; }
    public int NonBillable { get; set; }
    public decimal? CostPerLead { get; set; }

    [NotMapped]
	public decimal TotalAmount { get { return (CostPerLead ?? 0) * Billable; } }
}
