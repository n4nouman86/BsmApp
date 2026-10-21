using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace BlazorPortal.Components.Pages.WhitelistedIpPages;

public partial class IndexPage
{
    private List<WhitelistedIp>? WhitelistedIps { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        WhitelistedIps = await db.WhitelistedIps.OrderByDescending(x => x.CreatedOn).ToListAsync();
    }

    private async Task OpenCreate()
    {
        var result = await dialogService.OpenSideAsync<CreatePage>(
            "Add Whitelisted IP",
            options: new SideDialogOptions { Position = DialogPosition.Right, ShowMask = true, MinWidth = 450, Width = "500px" });

        if (result is int createdId)
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            var record = await db.WhitelistedIps.FindAsync(createdId);

            if (record is not null)
            {
                WhitelistedIps?.Insert(0, record);
                WhitelistedIps = WhitelistedIps is null ? null : new List<WhitelistedIp>(WhitelistedIps);
            }
        }
    }

    private async Task OpenEdit(int id)
    {
        var result = await dialogService.OpenSideAsync<EditPage>(
            "Edit Whitelisted IP",
            new Dictionary<string, object?> { { "Id", id } },
            new SideDialogOptions { Position = DialogPosition.Right, ShowMask = true, MinWidth = 450, Width = "500px" });

        if (result is int updatedId)
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            var record = await db.WhitelistedIps.FindAsync(updatedId);

            if (record is not null)
            {
                var index = WhitelistedIps?.FindIndex(x => x.Id == updatedId) ?? -1;
                if (index >= 0)
                {
                    WhitelistedIps![index] = record;
                    WhitelistedIps = new List<WhitelistedIp>(WhitelistedIps);
                }
            }
        }
    }

    private async Task DeleteWhitelistedIp(WhitelistedIp ip)
    {
        var confirmed = await dialogService.Confirm(
            $"Are you sure you want to delete '{ip.IpAddress}'?",
            "Delete Whitelisted IP",
            new ConfirmOptions { OkButtonText = "Delete", CancelButtonText = "Cancel" });

        if (confirmed != true) return;

        await using var db = await DbFactory.CreateDbContextAsync();
        db.WhitelistedIps.Remove(ip);
        await db.SaveChangesAsync();

        WhitelistedIps = await db.WhitelistedIps.OrderByDescending(x => x.CreatedOn).ToListAsync();
    }
}
