#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

[Keyless]
public partial class vwcTotalDayTransfer
{
    [Column(TypeName = "date")]
    public DateTime? TransferDate { get; set; }

    public int? Total { get; set; }
    public int? BillAble { get; set; }
    public int? NonBillAble { get; set; }
}