#nullable disable
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

[Keyless]
public partial class vwBonus
{
	[Unicode(false)]
	public string EmployeeName { get; set; }

	[Unicode(false)]
	public string AgentName { get; set; }

	public DateTime AgentOnboardOn { get; set; }

	public DateTime InvoiceServiceFrom { get; set; }

	public decimal InvoiceTotalCost { get; set; }
}
