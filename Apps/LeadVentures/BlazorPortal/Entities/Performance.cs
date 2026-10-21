#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorPortal.Entities;

public class Performance
{
    public int Id { get; set; }

    [Required]
    [Column(TypeName = "date")]
    public DateTime PerformancDate { get; set; }
	public int KGTarget { get; set; }
	public int KGReps { get; set; }
    public int KGVerifiers { get; set; }
	public int BSMTarget { get; set; }
	public int BSMReps { get; set; }
    public int BSMVerifiers { get; set; }
	public decimal BSMDialerCost { get; set; }
	public decimal KGDialerCost { get; set; }
	public decimal DurketaDialerCost { get; set; }
}
