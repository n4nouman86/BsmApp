using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace BlazorPortal.Components.Pages.InvoicePages;

public partial class InvoiceViewPage
{
    [Parameter]
    public Guid Id { get; set; }
    private Invoice Invoice { get; set; } = new Invoice();
    private InvoiceFrom InvoiceFrom { get; set; } = new InvoiceFrom();
    private Agent Agent { get; set; } = new Agent();
    private IEnumerable<InvoiceBank> InvoiceBanks { get; set; } = new List<InvoiceBank>();
    private int Counter { get; set; } = 1;




    protected override async Task OnInitializedAsync()
    {
        using var context = contextFactory.CreateDbContext();
        Invoice = await context.Invoices.FindAsync(Id) ?? new Invoice();
        InvoiceFrom = await context.InvoiceFroms.FindAsync(Invoice!.InvoiceFromId) ?? new InvoiceFrom();
        Agent = await context.Agents.FindAsync(Invoice.AgentId) ?? new Agent();

        FormattableString sql = @$"SELECT * FROM InvoiceBanks
                                    WHERE
	                                    InvoiceBanks.Id IN (SELECT BankId FROM BanksOfInvoices WHERE BanksOfInvoices.InvoiceId = {Invoice.Id})";

        InvoiceBanks = await context.InvoiceBanks.FromSql(sql).ToListAsync();
    }




    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            await JSRuntime.InvokeVoidAsync("PrintInvoice");
        }
    }
}