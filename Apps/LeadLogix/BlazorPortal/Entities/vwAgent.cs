#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

[Keyless]
public partial class vwAgent
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    [Unicode(false)]
    public string FullName { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string PhoneNumber { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string AgentEmail { get; set; }

    public int DailyCap { get; set; }

    public int CoolOffTime { get; set; }

    public bool IsOnline { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string CampaignStatus { get; set; }

    [Required]
    [StringLength(255)]
    [Unicode(false)]
    public string States { get; set; }

    public DateTimeOffset CreateOn { get; set; }
}