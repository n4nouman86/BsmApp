using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.InvoiceBankPages;

public partial class InvoiceBankCreatePage
{
    private InvoiceBank Input { get; set; } = new InvoiceBank() { InvoiceBankTypeId = 1 };
    private int BankTypeId { get; set; }
    private IEnumerable<InvoiceBankType>? BankTypes { get; set; }




    protected override async Task OnInitializedAsync()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        BankTypes = await db.InvoiceBankTypes.ToListAsync();
    }




    private async Task CreateBank()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        await db.InvoiceBanks.AddAsync(Input);
        await db.SaveChangesAsync();

        navManager.NavigateTo("InvoiceBankIndex");
    }
}