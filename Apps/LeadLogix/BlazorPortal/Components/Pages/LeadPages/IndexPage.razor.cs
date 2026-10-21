using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.LeadPages;

public partial class IndexPage
{
	private IEnumerable<LeadDispositionStatus>? Dispositions { get; set; }
	private IEnumerable<vwLead>? Leads { get; set; }
    private IEnumerable<vwdAgent>? Agents { get; set; }




    protected override async Task OnInitializedAsync()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        Agents = await db.vwdAgents.OrderBy(x => x.Text).ToListAsync();
        Dispositions = await db.LeadDispositionStatuses.OrderBy(x => x.Name).ToListAsync();
		await SearchLeads();
    }




    private InputSearch Input { get; set; } = new();
    private async Task SearchLeads()
    {
        Leads = null;

        await using var db = await DbFactory.CreateDbContextAsync();
        var query = db.vwLeads.AsQueryable();

        query = query.Where(x => x.AgentTransferOnDate >= Input.FromDate && x.AgentTransferOnDate <= Input.ToDate);

        if (Input.AgentId.HasValue)
        {
            query = query.Where(x => x.AgentId == Input.AgentId);
        }

		if (Input.DispositionId.HasValue)
		{
			query = query.Where(x => x.DispositionStatusId == Input.DispositionId);
		}

		Leads = await query.OrderBy(x => x.TransferOn).ToListAsync();
    }




    private InputPhoneSearch InputPhone { get; set; } = new();
    private async Task SearchPhone()
    {
        Leads = null;

        await using var db = await DbFactory.CreateDbContextAsync();
        Leads = await db.vwLeads
            .Where(x => x.PhoneNumber == InputPhone.PhoneNumber)
            .ToListAsync();
    }




    private sealed class InputSearch
    {
        public DateTime FromDate { get; set; } = DateTime.Now;
        public DateTime ToDate { get; set; } = DateTime.Now;
        public Guid? AgentId { get; set; }
        public int? DispositionId { get; set; }
	}




    private sealed class InputPhoneSearch
    {
        public string? PhoneNumber { get; set; }
    }
}