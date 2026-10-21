using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.InvoiceFromPages;

public partial class InvoiceFromEditPage
{
    [Parameter]
    public int Id { get; set; }

    [SupplyParameterFromForm]
    private InvoiceFrom? Input { get; set; }







    protected override async Task OnInitializedAsync()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        Input = await db.InvoiceFroms.FindAsync(Id);
    }







    private async Task UpdateInvoiceFrom()
    {
        if (Input is null) return;

        await using var db = await DbFactory.CreateDbContextAsync();
        db.Entry(Input).State = EntityState.Modified;
        await db.SaveChangesAsync();

        navManager.NavigateTo("InvoiceFromIndex");
    }
}