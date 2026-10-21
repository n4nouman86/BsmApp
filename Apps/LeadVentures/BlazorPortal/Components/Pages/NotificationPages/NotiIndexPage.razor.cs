using BlazorPortal.Data;
using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.NotificationPages;

public partial class NotiIndexPage
{
    private IEnumerable<Notification>? Notifications { get; set; }
    private ApplicationUser? AppUser { get; set; }




    protected override async Task OnInitializedAsync()
    {
        var user = await authProvider.GetAuthenticationStateAsync();
        AppUser = await userManager.GetUserAsync(user.User);

        await using var db = await DbFactory.CreateDbContextAsync();

        Notifications = await db.Notifications
            .Where(x => x.AppUserId == AppUser!.Id)
            .OrderByDescending(x => x.CreateOn)
            .Take(25)
            .ToListAsync();
    }




    private async Task GotoComment(int id, string url)
    {
        await using var db = await DbFactory.CreateDbContextAsync();

        await db.Notifications
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(setter =>
            setter.SetProperty(x => x.IsRead, true));

        navManager.NavigateTo(url);
    }




    private async Task MarkAllAsReadAsync()
    {
        if (AppUser is not null)
        {
            await using var db = await DbFactory.CreateDbContextAsync();

            await db.Notifications
                .Where(x => x.AppUserId == AppUser.Id)
                .ExecuteUpdateAsync(sett =>
                    sett.SetProperty(p => p.IsRead, true));

            if (Notifications is not null)
            {
                foreach (var item in Notifications)
                {
                    item.IsRead = true;
                }
            }
        }
    }
}