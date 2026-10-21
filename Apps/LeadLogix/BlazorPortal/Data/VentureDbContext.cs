using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Data;

public class VentureDbContext : DbContext
{
	public VentureDbContext(DbContextOptions<VentureDbContext> options) : base(options) { }
	
	public DbSet<vwLead> vwLeads { get; set; }
	public DbSet<DncLead> DncLeads { get; set; }
}
