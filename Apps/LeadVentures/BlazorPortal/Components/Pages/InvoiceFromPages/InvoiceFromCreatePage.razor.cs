using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;

namespace BlazorPortal.Components.Pages.InvoiceFromPages;

public partial class InvoiceFromCreatePage
{
    private InvoiceFrom Input { get; set; } = new InvoiceFrom();




    private async Task CreateInvoiceFrom()
    {
        using var context = contextFactory.CreateDbContext();
        await context.InvoiceFroms.AddAsync(Input);
        await context.SaveChangesAsync();

        navManager.NavigateTo("InvoiceFromIndex");
    }
}