using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.VerifierUserPages;

public partial class RecentTransferedPage
{
    private IEnumerable<vwLead>? Leads { get; set; }





    protected async override Task OnInitializedAsync()
    {
        var authState = await authProvider.GetAuthenticationStateAsync();
        var user = await userManager.GetUserAsync(authState.User);

        await using var db = await DbFactory.CreateDbContextAsync();
        Leads = await db.vwLeads
            .Where(x => x.VerifierId == user!.Id)
            .OrderByDescending(x => x.TransferOn)
            .Take(15)
            .ToListAsync();
    }
}