using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.InvoiceFromPages;


public partial class InvoiceFromIndexPage
{
    private IEnumerable<InvoiceFrom>? InvoiceFroms { get; set; }






    protected override async Task OnInitializedAsync()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        InvoiceFroms = await db.InvoiceFroms.ToListAsync();
    }
}