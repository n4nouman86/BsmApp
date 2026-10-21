using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace BlazorPortal.Components.Pages.InvoicePages;

public partial class CreatePage
{
    private Invoice Input { get; set; } = new()
    {
        ProjectDescription = "Live Qualified Transfers",
        DueDate = DateTime.Today.AddDays(2),
        ServiceFrom = GetLastWeekMonday(),
        ServiceTo = GetLastWeekMonday().AddDays(6),
        InvoiceFromId = 2
    };

    private static DateTime GetLastWeekMonday()
    {
        var today = DateTime.Today;
        var daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
        return today.AddDays(-(daysSinceMonday + 7));
    }

    private IEnumerable<vwdAgent>? Agents { get; set; }
    private IEnumerable<vwdInvoiceBank>? Banks { get; set; }
    private IEnumerable<InvoiceFrom>? InvoiceFroms { get; set; }
    private List<int> BankIds { get; set; } = new() { 3 };




    protected override async Task OnInitializedAsync()
    {
        using var context = contextFactory.CreateDbContext();
        Agents = await context.vwdAgents.OrderByDescending(x => x.Text).ToListAsync();
        Banks = await context.vwdInvoiceBanks.ToListAsync();
        InvoiceFroms = await context.InvoiceFroms.ToListAsync();
    }




    private async Task ChangeAgent()
    {
		Input.BillableLeads = 0;

        using var context = contextFactory.CreateDbContext();
        var agent = await context.Agents.FirstOrDefaultAsync(x => x.Id == Input.AgentId);

		if (agent?.CostPerLead.HasValue == true)
            Input.CostPerLead = agent.CostPerLead.Value;

        await LoadBillableLeads();
    }




    private async Task LoadBillableLeads()
    {
        if (Input.AgentId == Guid.Empty) return;

        using var context = contextFactory.CreateDbContext();
        var count = await context.vwLeads
            .Where(x => x.AgentTransferOnDate >= Input.ServiceFrom &&
                        x.AgentTransferOnDate <= Input.ServiceTo &&
                        x.AgentId == Input.AgentId &&
                        x.IsBillable == true)
            .CountAsync();

        Input.BillableLeads = count;
        notify.Notify(NotificationSeverity.Success, "Invoice", "Billable leads loaded!");
    }




    private async Task Save()
    {
        if (Input.AgentId == Guid.Empty)
        {
            notify.Notify(NotificationSeverity.Error, "Invoice", "Select agent!");
            return;
        }

        using var context = contextFactory.CreateDbContext();
        Input.Id = Guid.NewGuid();
        Input.CreateOn = DateTime.Now;
        Input.TotalCost = Input.CalculateTotalCost;

        var lastInvoice = await context.Invoices.OrderByDescending(x => x.InvoiceNo).FirstOrDefaultAsync();
        Input.InvoiceNo = lastInvoice is null ? 2000 : lastInvoice.InvoiceNo + 1;

        await context.Invoices.AddAsync(Input);
        await context.SaveChangesAsync();

        foreach (var bankId in BankIds)
            await context.BanksOfInvoices.AddAsync(new BanksOfInvoice { InvoiceId = Input.Id, BankId = bankId });

        await context.SaveChangesAsync();

        DialogService.CloseSide(Input.Id);
    }




    private void Cancel() => DialogService.CloseSide(false);
}
