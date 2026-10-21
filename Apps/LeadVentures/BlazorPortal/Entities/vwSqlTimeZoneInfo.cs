#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

[Keyless]
public partial class vwSqlTimeZoneInfo
{
    [Required]
    [Column("name")]
    [StringLength(128)]
    public string Name { get; set; }

    [Required]
    [Column("current_utc_offset")]
    [StringLength(6)]
    public string CurrentUtcOffset { get; set; }

    [Column("is_currently_dst")]
    public bool IsCurrentlyDst { get; set; }
}