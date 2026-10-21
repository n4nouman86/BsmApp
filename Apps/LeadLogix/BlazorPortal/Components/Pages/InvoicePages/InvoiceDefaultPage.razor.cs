using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace BlazorPortal.Components.Pages.InvoicePages;

public partial class InvoiceDefaultPage
{
    private List<vwInvoice>? Invoices { get; set; }




    protected override async Task OnInitializedAsync()
    {
        using var context = contextFactory.CreateDbContext();
        Invoices = await context.vwInvoices
            .Where(x => x.IsDefault == true)
            .OrderByDescending(x => x.DueDate)
            .ToListAsync();
    }




    private async Task SaveDefault(vwInvoice invoice)
    {
        var confirmed = await dialogService.OpenAsync<MarkDefaultDialog>(
            "Unmark as Default",
            new Dictionary<string, object?>
            {
                { "Invoice", invoice },
                { "ConfirmText", "Are you sure you want to unmark this invoice as default?" },
                { "ActionLabel", "Yes, Unmark" },
                { "ActionStyle", ButtonStyle.Warning }
            },
            new DialogOptions { Width = "400px", CloseDialogOnOverlayClick = true });

        if (confirmed is not true) return;

        using var context = contextFactory.CreateDbContext();
        var res = context.Invoices.Where(x => x.Id == invoice.Id)
            .ExecuteUpdate(s => s.SetProperty(x => x.IsDefault, false));

        if (res > 0)
        {
            Invoices?.RemoveAll(x => x.Id == invoice.Id);
            Invoices = new List<vwInvoice>(Invoices!);
        }
    }
}