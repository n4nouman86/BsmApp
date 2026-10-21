using BlazorPortal.Entities;
using Radzen;

namespace BlazorPortal.Components.Pages.WhitelistedIpPages;

public partial class CreatePage
{
    private WhitelistedIp Input { get; set; } = new();

    private async Task Save()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        await db.WhitelistedIps.AddAsync(Input);
        await db.SaveChangesAsync();

        DialogService.CloseSide(Input.Id);
    }

    private void Cancel() => DialogService.CloseSide(false);
}
