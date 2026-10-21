#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

[Keyless]
public partial class vwdActiveAgent
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(357)]
    [Unicode(false)]
    public string Text { get; set; }

    [StringLength(256)]
    public string Email { get; set; }

    public bool DoNotAllowDnc { get; set; }
}