using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.InvoiceBankPages;

public partial class InvoiceBankIndexPage
{
    private IEnumerable<InvoiceBank>? Banks { get; set; }






    protected override async Task OnInitializedAsync()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        Banks = await db.InvoiceBanks.ToListAsync();
    }
}