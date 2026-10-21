using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.InvoiceBankPages;

public partial class InvoiceBankEditPage
{
    [Parameter]
    public int Id { get; set; }

    private InvoiceBank? Input { get; set; } = new InvoiceBank();
    private IEnumerable<InvoiceBankType>? BankTypes { get; set; }




    protected override async Task OnInitializedAsync()
    {
        using var context = contextFactory.CreateDbContext();
        BankTypes = await context.InvoiceBankTypes.ToListAsync();

        Input = await context.InvoiceBanks.FindAsync(Id);
    }




    private async Task CreateBank()
    {
        using var context = contextFactory.CreateDbContext();
        if (Input is null) return;

        context.Entry(Input).State = EntityState.Modified;
        await context.SaveChangesAsync();

        navManager.NavigateTo("InvoiceBankIndex");
    }
}