#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

public partial class Agent
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(450)]
    public string AppUserId { get; set; }

    [Required]
    [StringLength(100)]
    [Unicode(false)]
    public string FullName { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string AgentEmail { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string PhoneNumber { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string Address { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string TransferNoDid { get; set; }

    [Column(TypeName = "money")]
    public decimal? CostPerLead { get; set; }

    public int DailyCap { get; set; }

    public int WeeklyCap { get; set; }

    public int MaxCap { get; set; }

    public int CoolOffTime { get; set; }

    public int CoolOffMaxLeads { get; set; }

    [StringLength(3000)]
    [Unicode(false)]
    public string ZipCodes { get; set; }

    [Required]
    [StringLength(255)]
    [Unicode(false)]
    public string States { get; set; }

    [Required]
    [StringLength(100)]
    [Unicode(false)]
    public string Country { get; set; }

    public bool IsOnline { get; set; }

    public bool DoNotAllowDnc { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string CampaignStatus { get; set; }

    [StringLength(3000)]
    [Unicode(false)]
    public string VerifierNotes { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string CustomLabel1 { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string CustomLabel2 { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string CustomLabel3 { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string CustomLabel4 { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string CustomLabel5 { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string CustomLabel6 { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string CustomLabel7 { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string CustomLabel8 { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string CustomLabel9 { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string CustomLabel10 { get; set; }

    public DateTimeOffset? PauseTill { get; set; }

    public DateTimeOffset? MonScheduleFrom { get; set; }

    public DateTimeOffset? TueScheduleFrom { get; set; }

    public DateTimeOffset? WedScheduleFrom { get; set; }

    public DateTimeOffset? ThuScheduleFrom { get; set; }

    public DateTimeOffset? FriScheduleFrom { get; set; }

    public DateTimeOffset? SatScheduleFrom { get; set; }

    public DateTimeOffset? SunScheduleFrom { get; set; }

    public DateTimeOffset? MonScheduleTo { get; set; }

    public DateTimeOffset? TueScheduleTo { get; set; }

    public DateTimeOffset? WedScheduleTo { get; set; }

    public DateTimeOffset? ThuScheduleTo { get; set; }

    public DateTimeOffset? FriScheduleTo { get; set; }

    public DateTimeOffset? SatScheduleTo { get; set; }

    public DateTimeOffset? SunScheduleTo { get; set; }

	public int? EmployeeId { get; set; }
	
    public DateTimeOffset CreateOn { get; set; }
}