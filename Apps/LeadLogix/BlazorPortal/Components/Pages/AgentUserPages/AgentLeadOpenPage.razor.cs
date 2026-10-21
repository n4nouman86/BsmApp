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
        await using var db = await DbFactory.CreateDbContextAsync();
        DispositionTypes = await db.LeadDispositionTypes.ToListAsync();
        Input = await db.vwLeads.FirstOrDefaultAsync(x => x.Id == Id) ?? new vwLead();
    }





    private async Task UpdateLead()
    {
        if (Input is null) return;

        await using var db = await DbFactory.CreateDbContextAsync();

        if (Input.DispositionTypeId.HasValue)
        {
            if (string.IsNullOrWhiteSpace(Input.DispositionComment))
            {
                notify.Notify(NotificationSeverity.Error, "Disposition", "Please share the reason for choosing the disposition.");
                return;
            }

            var lead = await db.Leads.FirstAsync(x => x.Id == Input.Id);

            lead.CallMemo = Input.CallMemo;
			lead.DispositionOn = DateTimeOffset.Now;
			lead.DispositionTypeId = Input.DispositionTypeId;
            lead.DispositionComment = Input.DispositionComment;
            lead.DispositionStatusId = 1;
            lead.IsBillable = false;

            db.Entry(lead).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        else
        {
            var lead = await db.Leads.FirstAsync(x => x.Id == Input.Id);

			lead.CallMemo = Input.CallMemo;
			lead.DispositionOn = null;
			lead.DispositionTypeId = null;
			lead.DispositionComment = null;
			lead.DispositionStatusId = null;
			lead.IsBillable = true;

            db.Entry(lead).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        navManager.NavigateTo("/AgentLead");
    }
}