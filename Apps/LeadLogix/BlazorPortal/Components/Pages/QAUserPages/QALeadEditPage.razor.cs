using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.QAUserPages;

public partial class QALeadEditPage
{
    [Parameter]
    public string Id { get; set; } = "";

    [SupplyParameterFromForm]
    private Lead? Input { get; set; }

    private vwLead? Lead { get; set; }  

    private IEnumerable<LeadDispositionStatus>? DispositionStatuses { get; set; }







    protected override async Task OnInitializedAsync()
    {
        var leadId = Guid.Parse(Id);

        await using var db = await DbFactory.CreateDbContextAsync();
        Input = await db.Leads.FindAsync(leadId);

        Lead = await db.vwLeads.FirstOrDefaultAsync(x => x.Id == leadId);

        DispositionStatuses = await db.LeadDispositionStatuses.ToListAsync();
    }









    private async Task UpdateLead()
    {
        if (Input is null) return;

        Input.IsBillable = true;

        if (Input.DispositionStatusId == 2)
        {
            Input.IsBillable = false;
        }

        await using var db = await DbFactory.CreateDbContextAsync();
        db.Entry(Input).State = EntityState.Modified;
        await db.SaveChangesAsync();

        navManager.NavigateTo("QALeadIndex");
    }
}