#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

[Keyless]
public partial class vwState
{
    public int Id { get; set; }

    [Required]
    [StringLength(102)]
    [Unicode(false)]
    public string Text { get; set; }
}