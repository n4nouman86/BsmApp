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
        await using var db = await DbFactory.CreateDbContextAsync();
        BankTypes = await db.InvoiceBankTypes.ToListAsync();

        Input = await db.InvoiceBanks.FindAsync(Id);
    }




    private async Task CreateBank()
    {
        if (Input is null) return;

        await using var db = await DbFactory.CreateDbContextAsync();
        db.Entry(Input).State = EntityState.Modified;
        await db.SaveChangesAsync();

        navManager.NavigateTo("InvoiceBankIndex");
    }
}