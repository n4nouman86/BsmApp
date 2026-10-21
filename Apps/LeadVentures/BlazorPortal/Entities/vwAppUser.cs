#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

[Keyless]
public partial class vwAppUser
{
    [Required]
    [StringLength(450)]
    public string Id { get; set; }

    [StringLength(256)]
    public string UserName { get; set; }

    [StringLength(256)]
    public string Email { get; set; }

    [Required]
    [StringLength(100)]
    [Unicode(false)]
    public string FullName { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public int AccessFailedCount { get; set; }

    [StringLength(4000)]
    public string Roles { get; set; }

    [Required]
    [StringLength(100)]
    [Unicode(false)]
    public string TimeZoneInfoId { get; set; }

    public DateTimeOffset? LastOnlineOn { get; set; }

    public DateTimeOffset CreateOn { get; set; }
}