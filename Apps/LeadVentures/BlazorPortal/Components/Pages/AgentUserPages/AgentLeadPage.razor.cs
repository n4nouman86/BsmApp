using BlazorPortal.Data;
using BlazorPortal.Entities;
using BlazorPortal.MethodExtensions;
using BlazorPortal.Services;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.AgentUserPages;

public partial class AgentLeadPage : IDisposable
{
    private List<vwLead>? Leads { get; set; }
    private Agent? Agent { get; set; }
    private ApplicationUser? AppUser { get; set; }
    private IEnumerable<LeadDispositionStatus>? DispositionStatuses { get; set; }




    protected override async Task OnInitializedAsync()
    {
        using var context = contextFactory.CreateDbContext();
        var authState = await authProvider.GetAuthenticationStateAsync();

        AppUser = await userManager.GetUserAsync(authState.User);

        Agent = await context.Agents.FirstOrDefaultAsync(x => x.AppUserId == AppUser!.Id);

        DispositionStatuses = await context.LeadDispositionStatuses.OrderBy(x => x.Name).ToListAsync();

        isAgentActive = Agent!.IsOnline;

        await RefreshTodayTransfer(false);

        Input.FromDate = Input.ToDate = DateTimeOffset.Now.ToLacalDateTimeOffset(AppUser!.TimeZoneInfoId).DateTime;

        if (Agent.PauseTimer > 0)
        {
            StartTimer();
        }
    }




    private bool isAgentActive = true;

    private async Task AgentStatusChange()
    {
        using var context = contextFactory.CreateDbContext();
        var authState = await authProvider.GetAuthenticationStateAsync();
        var user = await userManager.GetUserAsync(authState.User);
        var agent = await context.Agents.FirstOrDefaultAsync(x => x.AppUserId == user!.Id);

        await context.Agents
            .Where(x => x.Id == agent!.Id)
            .ExecuteUpdateAsync(prop =>
                prop.SetProperty(x => x.IsOnline, isAgentActive));

        notify.Notify(Radzen.NotificationSeverity.Success, "Status", "Updated successfully!");
    }




    private async Task RefreshTodayTransfer(bool showNoti = true)
    {
        using var context = contextFactory.CreateDbContext();
        DateTimeOffset today = DateTimeOffset.Now.ToLacalDateTimeOffset(AppUser!.TimeZoneInfoId);

        Leads = await context.vwLeads
            .Where(x => x.AgentId == Agent!.Id && x.AgentTransferOnDate == today.Date)
            .OrderByDescending(x => x.TransferOn)
            .ToListAsync();

        if (showNoti)
        {
            notify.Notify(Radzen.NotificationSeverity.Success, "Lead", "Updated successfully!");
        }
    }




    private int PauseTill { get; set; } = 0;
    
    private async Task UpdatePause()
    {
        using var context = contextFactory.CreateDbContext();
        if (Agent is null) return;

        Agent.PauseTill = DateTimeOffset.Now.ToServerDateTimeOffset().AddMinutes(PauseTill);
        
        context.Entry(Agent).State = EntityState.Modified;
        await context.SaveChangesAsync();

        if (Agent.PauseTimer > 0)
        {
            StartTimer();
        }

        notify.Notify(Radzen.NotificationSeverity.Success, "Agent", "Updated successfully!");
    }




    private async Task CancelPause()
    {
        using var context = contextFactory.CreateDbContext();
        if (Agent is null) return;

        Agent.PauseTill = null;

        context.Entry(Agent).State = EntityState.Modified;
        await context.SaveChangesAsync();

        timer?.Dispose();

        notify.Notify(Radzen.NotificationSeverity.Success, "Agent", "Updated successfully!");
    }




    private Timer? timer;
    
    private void StartTimer()
    {
        timer = new Timer(async _ =>
        {
           await InvokeAsync(StateHasChanged);
        }, null, 0, 100);
    }




    private InputSearch Input { get; set; } = new();
    
    private async Task SearchLeads()
    {
        using var context = contextFactory.CreateDbContext();
        var query = context.vwLeads.AsQueryable();

        query = query.Where(x => x.AgentId == Agent!.Id && x.AgentTransferOnDate >= Input.FromDate && x.AgentTransferOnDate <= Input.ToDate);

        if (Input.Billable)
        {
            query = query.Where(x => x.IsBillable == true);
        }

        if (Input.StatusId is not null)
        {
            query.Where(x => x.DispositionStatusId == Input.StatusId);
        }

        Leads = await query.OrderByDescending(x => x.TransferOn).ToListAsync();
    }




    private async Task SearchByName()
    {
        using var context = contextFactory.CreateDbContext();
        Leads = await context.vwLeads
            .Where(x => x.AgentId == Agent!.Id && x.FullName.Contains(Input.FullName))
            .OrderByDescending(x => x.TransferOn)
            .ToListAsync();
    }




    private async Task SearchByPhone()
    {
        using var context = contextFactory.CreateDbContext();
        Leads = await context
            .vwLeads
            .Where(x => x.AgentId == Agent!.Id && x.PhoneNumber.Contains(Input.PhoneNumber))
            .OrderByDescending(x => x.TransferOn)
            .ToListAsync();
    }




    private void ExportCSV()
    {
        if (Leads is not null)
        {
            CsvLeadExportService csvExporter = new CsvLeadExportService();
            csvExporter.CreateCsv(JS, Leads, AppUser!.TimeZoneInfoId);
        }
    }




    public void Dispose()
    {
        timer?.Dispose();
    }




    private sealed class InputSearch
    {
        public DateTime FromDate { get; set; } = DateTime.UtcNow;
        public DateTime ToDate { get; set; } = DateTime.UtcNow;
        public int? StatusId { get; set; }
        public bool Billable { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}