#nullable disable
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorPortal.Entities;

public class PerformanceLine
{
    public int Id { get; set; }
    public int PerformanceId { get; set; }
    public DateTime TransferTime { get; set; }
	public int? KgBestDay { get; set; }
	public int? KgTransfers { get; set; }
	public int? BsmBestDay { get; set; }
	public int? BsmTransfers { get; set; }
}
