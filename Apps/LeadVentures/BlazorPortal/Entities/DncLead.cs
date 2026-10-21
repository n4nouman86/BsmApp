#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

[Table("DncLeads")]
public partial class DncLead
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string Phone { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string FirstName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string LastName { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string Address1 { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string City { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string State { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string PostalCode { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Email { get; set; }

	public DateTime CreatedOn { get; set; }
}
