using BlazorPortal.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace BlazorPortal.Data;

public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public string CurrentUserName { get; set; } = "Anonymous";

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Agent> Agents { get; set; }

    public DbSet<BanksOfInvoice> BanksOfInvoices { get; set; }

    public DbSet<Invoice> Invoices { get; set; }

    public DbSet<InvoiceBank> InvoiceBanks { get; set; }

    public DbSet<InvoiceBankType> InvoiceBankTypes { get; set; }

    public DbSet<InvoiceFrom> InvoiceFroms { get; set; }

    public DbSet<Lead> Leads { get; set; }

    public DbSet<CalledLead> CalledLeads { get; set; }

    public DbSet<vwCalledLead> vwCalledLeads { get; set; }

    public DbSet<LeadComment> LeadComments { get; set; }

    public DbSet<LeadDispositionStatus> LeadDispositionStatuses { get; set; }

    public DbSet<LeadDispositionType> LeadDispositionTypes { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<State> States { get; set; }

    public DbSet<vwAgent> vwAgents { get; set; }

    public DbSet<vwAppUser> vwAppUsers { get; set; }

    public DbSet<vwInvoice> vwInvoices { get; set; }

    public DbSet<vwBonus> vwBonuses { get; set; }

    public DbSet<vwLead> vwLeads { get; set; }

    public DbSet<vwLeadComment> vwLeadComments { get; set; }

    public DbSet<vwSqlTimeZoneInfo> vwSqlTimeZoneInfoes { get; set; }

    public DbSet<vwState> vwStates { get; set; }

    public DbSet<vwStatusAgent> vwStatusAgents { get; set; }

    public DbSet<vwTotalTodayTransferReport> vwTotalTodayTransferReports { get; set; }

    public DbSet<vwUpcomingInvoice> vwUpcomingInvoices { get; set; }

    public DbSet<vwcDayTransfer> vwcDayTransfers { get; set; }

    public DbSet<vwcTotalDayTransfer> vwcTotalDayTransfers { get; set; }

    public DbSet<vwdActiveAgent> vwdActiveAgents { get; set; }

    public DbSet<vwdAgent> vwdAgents { get; set; }

    public DbSet<vwdAgentAppUser> vwdAgentAppUsers { get; set; }

    public DbSet<vwdInvoiceBank> vwdInvoiceBanks { get; set; }

    public  DbSet<AuditLog> AuditLogs { get; set; }

	public DbSet<LeadSetting> LeadSettings { get; set; }

	public DbSet<WhitelistedIp> WhitelistedIps { get; set; }

	public DbSet<Employee> Employees { get; set; }


	public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
		var entries = ChangeTracker
			.Entries()
			.Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
			.ToList();

		List<AuditLog> logs = new();

		foreach (var entry in entries)
		{
			var json = JsonSerializer.Serialize(
				entry.Properties.ToDictionary(
					p => p.Metadata.Name,
					p => p.CurrentValue
				));

			var log = new AuditLog
			{
				EntityName = entry.Entity.GetType().Name,
				EntityId = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString() ?? "N/A",
				EntityState = entry.State.ToString(),
				EntityMetadata = json,
				EntityStateOn = DateTime.Now,
				UserName = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? CurrentUserName,
				Memo = string.Empty
			};

			logs.Add(log);
		}

		if (logs.Count > 0)
		{
			AuditLogs.AddRange(logs);
		}

		return await base.SaveChangesAsync(cancellationToken);
	}
}

