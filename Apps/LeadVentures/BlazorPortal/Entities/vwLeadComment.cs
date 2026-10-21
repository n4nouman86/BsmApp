#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

[Keyless]
public partial class vwLeadComment
{
    public int Id { get; set; }

    public Guid LeadId { get; set; }

    [Required]
    [StringLength(450)]
    public string AppUserId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string FullName { get; set; }

    public DateTimeOffset CommentOn { get; set; }

    [Required]
    [StringLength(3000)]
    [Unicode(false)]
    public string Comment { get; set; }
}