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
        using var context = contextFactory.CreateDbContext();
        BankTypes = await context.InvoiceBankTypes.ToListAsync();
    }




    private async Task CreateBank()
    {
        using var context = contextFactory.CreateDbContext();
        await context.InvoiceBanks.AddAsync(Input);
        await context.SaveChangesAsync();

        navManager.NavigateTo("InvoiceBankIndex"); 
    }
}