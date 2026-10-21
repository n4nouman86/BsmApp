#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

[Keyless]
public partial class vwdAgentAppUser
{
    [Required]
    [StringLength(450)]
    public string Id { get; set; }

    [StringLength(359)]
    public string Text { get; set; }
}