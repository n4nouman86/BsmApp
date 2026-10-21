using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.LeadPages;

public partial class EditPage
{
    [Parameter]
    public string Id { get; set; } = "";
    private Lead? Input { get; set; }

    private Agent? Agent { get; set; } = new Agent();
    private IEnumerable<vwdAgent>? Agents { get; set; }
    private IEnumerable<LeadDispositionType>? DispositionTypes { get; set; }
    private IEnumerable<LeadDispositionStatus>? DispositionStatuses { get; set; }
    private IEnumerable<vwState>? States { get; set; }




    protected override async Task OnInitializedAsync()
    {
        var leadId = Guid.Parse(Id);

        await using var context = await DbFactory.CreateDbContextAsync();

        Agents = await context.vwdAgents.OrderByDescending(x => x.Text).ToListAsync();
        DispositionTypes = await context.LeadDispositionTypes.ToListAsync();
        DispositionStatuses = await context.LeadDispositionStatuses.ToListAsync();
        States = await context.vwStates.ToListAsync();

        Input = await context.Leads.FindAsync(leadId) ?? new Lead();

        Agent = await context.Agents.FindAsync(Input.AgentId);
    }




    bool isUpdating = false;
    private async Task UpdateLead()
    {
        isUpdating = true;
        await InvokeAsync(StateHasChanged);

        if (Input is null) return;

        Input.IsBillable = true;

        if (Input.DispositionStatusId == 2)
        {
            Input.IsBillable = false;
        }

        await using var context = await DbFactory.CreateDbContextAsync();

        context.Entry(Input).State = EntityState.Modified;
        await context.SaveChangesAsync();

        isUpdating = false;
        notiService.Notify(Radzen.NotificationSeverity.Success, "Success", "Lead updated successfully.", TimeSpan.FromHours(3));
	}
}