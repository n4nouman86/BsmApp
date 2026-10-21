#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

public partial class CalledLead
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	[Required]
	[StringLength(450)]
	public string VerifierId { get; set; }

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
	[StringLength(256)]
	[Unicode(false)]
	public string Address { get; set; }

	public int? StateId { get; set; }

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
	[StringLength(256)]
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

	public DateTimeOffset CalledOn { get; set; }

	[Required]
	[StringLength(256)]
	[Unicode(false)]
	public string Status { get; set; }
}
