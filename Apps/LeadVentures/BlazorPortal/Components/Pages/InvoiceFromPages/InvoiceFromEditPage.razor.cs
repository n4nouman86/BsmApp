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
        using var context = contextFactory.CreateDbContext();
        Input = await context.InvoiceFroms.FindAsync(Id);
    }







    private async Task UpdateInvoiceFrom()
    {
        using var context = contextFactory.CreateDbContext();
        if (Input is null) return;

        context.Entry(Input).State = EntityState.Modified;
        await context.SaveChangesAsync();

        navManager.NavigateTo("InvoiceFromIndex");
    }
}