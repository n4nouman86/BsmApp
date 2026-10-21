#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

public partial class InvoiceFrom
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string InvoiceBy { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string MobileNumber { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string LandLineNumber { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string EmailAddress { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string Address { get; set; }
}