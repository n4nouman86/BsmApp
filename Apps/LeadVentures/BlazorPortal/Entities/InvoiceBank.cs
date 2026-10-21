#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

public partial class InvoiceBank
{
    [Key]
    public int Id { get; set; }

    public int InvoiceBankTypeId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string BankName { get; set; }

    [StringLength(250)]
    [Unicode(false)]
    public string BankAddress { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Routing { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string AccountNumber { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string AccountType { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string BeneficiaryName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Currency { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string BranchAddress { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string AccountTitle { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string IbanorAccountNumber { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string SwiftCode { get; set; }

    [StringLength(250)]
    [Unicode(false)]
    public string Guidlines { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string ContractName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Email { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string ContactName { get; set; }

    [StringLength(256)]
    public string PaymentLink { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string BankCode { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string BranchCode { get; set; }
}