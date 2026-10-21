using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.VerifierUserPages;

public partial class RecentTransferedPage
{
    private IEnumerable<vwLead>? Leads { get; set; }





    protected async override Task OnInitializedAsync()
    {
        using var context = contextFactory.CreateDbContext();
        var authState = await authProvider.GetAuthenticationStateAsync();
        var user = await userManager.GetUserAsync(authState.User);

        Leads = await context.vwLeads
            .Where(x => x.VerifierId == user!.Id)
            .OrderByDescending(x => x.TransferOn)
            .Take(15)
            .ToListAsync();
    }
}