#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

[Keyless]
public partial class vwTotalTodayTransferReport
{
    public int? Target { get; set; }

    public int? Transfer { get; set; }

    public int? UnderReview { get; set; }

    public int? Approved { get; set; }

    public int? NotApproved { get; set; }
}