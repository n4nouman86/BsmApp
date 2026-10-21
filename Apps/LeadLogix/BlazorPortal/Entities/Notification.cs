#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

public partial class Notification
{
    [Key]
    public int Id { get; set; }

    public DateTimeOffset CreateOn { get; set; }

    [Required]
    [StringLength(450)]
    public string AppUserId { get; set; }

    [Required]
    [StringLength(100)]
    [Unicode(false)]
    public string NotiText { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string NotiUrl { get; set; }

    public bool IsRead { get; set; }
}