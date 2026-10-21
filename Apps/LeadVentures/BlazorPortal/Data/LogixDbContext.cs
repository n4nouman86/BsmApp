using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Data;

public class LogixDbContext : DbContext
{
	public LogixDbContext(DbContextOptions<LogixDbContext> options) : base(options) {}

	public DbSet<vwLead> vwLeads { get; set; }
}
