using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.DashboardPages;


public partial class DashboardAgent
{
    private IEnumerable<vwcDayTransfer>? ChartData { get; set; }


    
    
    protected override async Task OnInitializedAsync()
    {
        var authState = await authProvider.GetAuthenticationStateAsync();
        var user = await userManager.GetUserAsync(authState.User);

        await using var db = await DbFactory.CreateDbContextAsync();
        var agent = await db.Agents.FirstOrDefaultAsync(x => x.AppUserId == user!.Id);

        ChartData = await db
            .vwcDayTransfers
            .Where(x => x.AgentId == agent!.Id)
            .OrderByDescending(x => x.TransferDate)
            .Take(20)
            .ToListAsync();
    }
}