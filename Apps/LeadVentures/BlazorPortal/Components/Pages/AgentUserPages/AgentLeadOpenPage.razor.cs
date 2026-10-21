using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace BlazorPortal.Components.Pages.AgentUserPages;

public partial class AgentLeadOpenPage
{
    [Parameter]
    public Guid Id { get; set; }

    private vwLead? Input { get; set; }
    private IEnumerable<LeadDispositionType>? DispositionTypes { get; set; }




    protected override async Task OnInitializedAsync()
    {
        using var context = contextFactory.CreateDbContext();
        DispositionTypes = await context.LeadDispositionTypes.ToListAsync();
        Input = await context.vwLeads.FirstOrDefaultAsync(x => x.Id ==Id) ?? new vwLead();
    }





    private async Task UpdateLead()
    {
        using var context = contextFactory.CreateDbContext();

        if (Input is null) return;

        if (Input.DispositionTypeId.HasValue)
        {
            if (string.IsNullOrWhiteSpace(Input.DispositionComment))
            {
                notify.Notify(NotificationSeverity.Error, "Disposition", "Please share the reason for choosing the disposition.");
                return;
            }

            var lead = await context.Leads.FirstAsync(x => x.Id == Input.Id);

            lead.CallMemo = Input.CallMemo;
			lead.DispositionOn = DateTimeOffset.Now;
			lead.DispositionTypeId = Input.DispositionTypeId;
			lead.DispositionComment = Input.DispositionComment;
			lead.DispositionStatusId = 1;
			lead.IsBillable = false;

            context.Entry(lead).State = EntityState.Modified;
			await context.SaveChangesAsync();
        }
        else
        {
            var lead = await context.Leads.FirstAsync(x => x.Id == Input.Id); 
            
            lead.CallMemo = Input.CallMemo; 
            lead.DispositionOn = null; 
            lead.DispositionTypeId = null; 
            lead.DispositionComment = null; 
            lead.DispositionStatusId = null;
            lead.IsBillable = true;

			context.Entry(lead).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        navManager.NavigateTo("/AgentLead");
    }
}