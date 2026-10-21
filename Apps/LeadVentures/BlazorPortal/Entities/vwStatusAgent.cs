#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

[Keyless]
public partial class vwStatusAgent
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(450)]
    public string AppUserId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string TimeZoneInfoId { get; set; }

    [Required]
    [StringLength(100)]
    [Unicode(false)]
    public string FullName { get; set; }

    public bool IsOnline { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string CampaignStatus { get; set; }

    public DateTimeOffset? ScheduleFrom { get; set; }

    public DateTimeOffset? ScheduleTo { get; set; }

    public DateTimeOffset? PauseTill { get; set; }

    public int DailyCap { get; set; }

    public int? TodayTransfer { get; set; }

    public int CoolOffTime { get; set; }

    public int? LastTransferMinsAgo { get; set; }

    [Required]
    [StringLength(255)]
    [Unicode(false)]
    public string States { get; set; }
}