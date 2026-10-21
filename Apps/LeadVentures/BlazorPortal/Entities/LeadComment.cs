#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

public partial class LeadComment
{
    [Key]
    public int Id { get; set; }

    public Guid LeadId { get; set; }

    [Required]
    [StringLength(450)]
    public string AppUserId { get; set; }

    public DateTimeOffset CommentOn { get; set; }

    [Required]
    [StringLength(3000)]
    [Unicode(false)]
    public string Comment { get; set; }
}