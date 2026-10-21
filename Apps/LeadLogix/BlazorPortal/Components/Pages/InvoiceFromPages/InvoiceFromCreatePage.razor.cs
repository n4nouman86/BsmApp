using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;

namespace BlazorPortal.Components.Pages.InvoiceFromPages;

public partial class InvoiceFromCreatePage
{
    private InvoiceFrom Input { get; set; } = new InvoiceFrom();




    private async Task CreateInvoiceFrom()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        await db.InvoiceFroms.AddAsync(Input);
        await db.SaveChangesAsync();

        navManager.NavigateTo("InvoiceFromIndex");
    }
}