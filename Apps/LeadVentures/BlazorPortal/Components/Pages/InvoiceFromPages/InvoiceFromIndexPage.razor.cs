using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.InvoiceFromPages;


public partial class InvoiceFromIndexPage
{
    private IEnumerable<InvoiceFrom>? InvoiceFroms { get; set; }






    protected override async Task OnInitializedAsync()
    {
        using var context = contextFactory.CreateDbContext();
        InvoiceFroms = await context.InvoiceFroms.ToListAsync();
    }
}