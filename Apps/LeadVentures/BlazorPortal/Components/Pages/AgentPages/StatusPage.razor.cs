using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.AgentPages;

public partial class StatusPage : IDisposable
{
    private IEnumerable<vwStatusAgent>? TotalAgents { get; set; }
    private vwTotalTodayTransferReport? TotalTransfer { get; set; }


    private string viewAs = "card";




    protected override void OnInitialized()
    {
        InitAgentTimer();
    }




    private Timer? agentTimer;
    private void InitAgentTimer()
    {
        agentTimer = new Timer(async (arg) =>
        {
            await GetAgents();
            await using var db = await DbFactory.CreateDbContextAsync();
            TotalTransfer = await db.vwTotalTodayTransferReports.FirstOrDefaultAsync();
            await InvokeAsync(StateHasChanged);
        }, null, 0, 15000);
    }




    private string AgentFilterValue { get; set; } = "All";
    private async Task GetAgents()
    {
        await using var db = await DbFactory.CreateDbContextAsync();

        if (AgentFilterValue == "All")
        {
            TotalAgents = await db.vwStatusAgents.ToListAsync();
        }

        if (AgentFilterValue == "Online")
        {
            FormattableString sql = @$"SELECT * FROM vwStatusAgents
                                        WHERE
	                                        vwStatusAgents.Id IN (SELECT vwdActiveAgents.Id FROM vwdActiveAgents)";

            TotalAgents = await db.vwStatusAgents.FromSql(sql).ToListAsync();
        }

        if (AgentFilterValue == "Offline")
        {
            FormattableString sql = @$"SELECT * FROM vwStatusAgents
                                        WHERE
	                                        vwStatusAgents.Id NOT IN (SELECT vwdActiveAgents.Id FROM vwdActiveAgents)";

            TotalAgents = await db.vwStatusAgents.FromSql(sql).ToListAsync();
        }

        if (SearchAgents is not null && SearchAgents.Any() && TotalAgents is not null && TotalAgents.Any())
        {
            SearchAgents = TotalAgents.Where(x => SearchAgents.All(y => y.Id == x.Id)).ToList();
        }
    }




    private IEnumerable<vwStatusAgent>? SearchAgents { get; set; }
    private void SearchAgent(string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            SearchAgents = null;
        }
        else
        {
            if (TotalAgents is not null) 
            {
                SearchAgents = TotalAgents.Where(x => x.FullName.ToLower().Contains(searchText.ToLower())).ToList();
            }
        }
    }




    public void Dispose()
    {
        agentTimer?.Dispose();
    }
}