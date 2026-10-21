using BlazorPortal.Data;
using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlazorPortal.Components.Pages.LeadPages;

public partial class CreatePage
{
    private Lead Input { get; set; } = new Lead() { TransferOn = DateTime.Now };
    
    private IEnumerable<vwdAgent>? Agents { get; set; }
    private IEnumerable<LeadDispositionType>? DispositionTypes { get; set; }
    private IEnumerable<LeadDispositionStatus>? DispositionStatuses { get; set; }
    private IEnumerable<vwState>? States { get; set; }

    private ApplicationUser? AppUser { get; set; }




    protected override async Task OnInitializedAsync()
    {
        await using var context = await DbFactory.CreateDbContextAsync();

        Agents = await context.vwdAgents.ToListAsync();
        DispositionTypes = await context.LeadDispositionTypes.ToListAsync();
        DispositionStatuses = await context.LeadDispositionStatuses.ToListAsync();
        States = await context.vwStates.ToListAsync();

        var authState = await authProvider.GetAuthenticationStateAsync();

        Input.VerifierId = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }




    private Agent? Agent { get; set; }
    private async Task AgentChanged()
    {
        await using var context = await DbFactory.CreateDbContextAsync();
 
        Agent = await context.Agents.FindAsync(Input.AgentId);

        if (Agent is null)
        {
            Input.CustomLabel1 = null;
            Input.CustomLabel2 = null;
            Input.CustomLabel3 = null;
            Input.CustomLabel4 = null;
            Input.CustomLabel5 = null;
            Input.CustomLabel6 = null;
            Input.CustomLabel7 = null;
            Input.CustomLabel8 = null;
            Input.CustomLabel9 = null;
            Input.CustomLabel10 = null;
        }
        else
        {
            Input.CustomLabel1 = Agent.CustomLabel1;
            Input.CustomLabel2 = Agent.CustomLabel2;
            Input.CustomLabel3 = Agent.CustomLabel3;
            Input.CustomLabel4 = Agent.CustomLabel4;
            Input.CustomLabel5 = Agent.CustomLabel5;
            Input.CustomLabel6 = Agent.CustomLabel6;
            Input.CustomLabel7 = Agent.CustomLabel7;
            Input.CustomLabel8 = Agent.CustomLabel8;
            Input.CustomLabel9 = Agent.CustomLabel9;
            Input.CustomLabel10 = Agent.CustomLabel10;

            var st = States?.FirstOrDefault(x => x.Text.Contains(Agent.States));

            if (st is not null)
            {
                Input.StateId = st.Id;
            }
        }
    }




    bool isCreating = false;
    private async Task CreateLead()
    {
        isCreating = true;
        await InvokeAsync(StateHasChanged);

        Input.Id = Guid.NewGuid();

        await using var context = await DbFactory.CreateDbContextAsync();

        await context.Leads.AddAsync(Input);
        await context.SaveChangesAsync();

		var authState = await authProvider.GetAuthenticationStateAsync();

		Input = new Lead()
        {
            TransferOn = DateTime.Now,
			VerifierId = authState.User.FindFirstValue(ClaimTypes.NameIdentifier) 
        };

        notify.Notify(Radzen.NotificationSeverity.Success, "Lead", "Created successfully!");
        isCreating = false;
    }
}