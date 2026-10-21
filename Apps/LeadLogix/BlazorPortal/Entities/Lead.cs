#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

public partial class Lead
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(450)]
    public string VerifierId { get; set; }

    public Guid AgentId { get; set; }

    [Required]
    [StringLength(256)]
    [Unicode(false)]
    public string PhoneNumber { get; set; }

    [Required]
    [StringLength(256)]
    [Unicode(false)]
    public string FullName { get; set; }

    [Required]
    [StringLength(256)]
    [Unicode(false)]
    public string DateOfBirth { get; set; }

    [Required]
    [StringLength(512)]
    [Unicode(false)]
    public string Address { get; set; }

    public int StateId { get; set; }

    [Required]
    [StringLength(256)]
    [Unicode(false)]
    public string Spouse { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string SpouseName { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string SpouseDob { get; set; }

    [Required]
    [StringLength(256)]
    [Unicode(false)]
    public string CurrentAutoCarrier { get; set; }

    [Required]
    [StringLength(256)]
    [Unicode(false)]
    public string AnyLapses { get; set; }

    [Required]
    [StringLength(2048)]
    [Unicode(false)]
    public string MakeAndModel { get; set; }

    [Required]
    [StringLength(256)]
    [Unicode(false)]
    public string Accidents { get; set; }

    [Required]
    [StringLength(256)]
    [Unicode(false)]
    public string Tickets { get; set; }

    [Required]
    [StringLength(256)]
    [Unicode(false)]
    public string HomeOwner { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string HomeType { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string HaveHomeClaims { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string CurrentHomeCarrier { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string AnyClaimInPastThreeYears { get; set; }

    public DateTimeOffset TransferOn { get; set; }

    public bool IsDncLead { get; set; }

    [StringLength(1024)]
    [Unicode(false)]
    public string CallMemo { get; set; }

    public int? DispositionTypeId { get; set; }

    public DateTimeOffset? DispositionOn { get; set; }

    [StringLength(1024)]
    [Unicode(false)]
    public string DispositionComment { get; set; }

    [StringLength(1024)]
    [Unicode(false)]
    public string VerifierComment { get; set; }

    public int? DispositionStatusId { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string CustomLabel1 { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string CustomLabel2 { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string CustomLabel3 { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string CustomLabel4 { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string CustomLabel5 { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string CustomLabel6 { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string CustomLabel7 { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string CustomLabel8 { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string CustomLabel9 { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string CustomLabel10 { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string Custom1 { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string Custom2 { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string Custom3 { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string Custom4 { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string Custom5 { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string Custom6 { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string Custom7 { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string Custom8 { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string Custom9 { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string Custom10 { get; set; }

    public bool IsBillable { get; set; }
}