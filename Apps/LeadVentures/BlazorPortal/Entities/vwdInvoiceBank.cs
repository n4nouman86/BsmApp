#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

[Keyless]
public partial class vwdInvoiceBank
{
    public int Id { get; set; }

    [StringLength(202)]
    [Unicode(false)]
    public string Text { get; set; }
}