using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace BlazorPortal.Components.Pages.InvoicePages;

public partial class EditPage
{
    [Parameter] public Guid InvoiceId { get; set; }

    private Invoice? Input { get; set; }
    private IEnumerable<vwdAgent>? Agents { get; set; }
    private IEnumerable<vwdInvoiceBank>? Banks { get; set; }
    private IEnumerable<InvoiceFrom>? InvoiceFroms { get; set; }
    private List<int> BankIds { get; set; } = new();




    protected override async Task OnInitializedAsync()
    {
        using var context = contextFactory.CreateDbContext();
        Agents = await context.vwdAgents.OrderByDescending(x => x.Text).ToListAsync();
        Banks = await context.vwdInvoiceBanks.ToListAsync();
        InvoiceFroms = await context.InvoiceFroms.ToListAsync();

        Input = await context.Invoices.FindAsync(InvoiceId);
        BankIds = await context.BanksOfInvoices
            .Where(x => x.InvoiceId == InvoiceId)
            .Select(x => x.BankId)
            .ToListAsync();
    }




    private async Task LoadBillableLeads()
    {
        if (Input is null || Input.AgentId == Guid.Empty) return;

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
        if (Input is null || Input.AgentId == Guid.Empty) return;

        using var context = contextFactory.CreateDbContext();
        Input.TotalCost = Input.CalculateTotalCost;

        var existing = await context.BanksOfInvoices.Where(x => x.InvoiceId == Input.Id).ToListAsync();
        context.BanksOfInvoices.RemoveRange(existing);

        foreach (var bankId in BankIds)
            await context.BanksOfInvoices.AddAsync(new BanksOfInvoice { InvoiceId = Input.Id, BankId = bankId });

        context.Entry(Input).State = EntityState.Modified;
        await context.SaveChangesAsync();

        DialogService.CloseSide(Input.Id);
    }




    private void Cancel() => DialogService.CloseSide(false);
}
