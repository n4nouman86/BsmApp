#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

public partial class BanksOfInvoice
{
    [Key]
    public int Id { get; set; }

    public Guid InvoiceId { get; set; }

    public int BankId { get; set; }
}