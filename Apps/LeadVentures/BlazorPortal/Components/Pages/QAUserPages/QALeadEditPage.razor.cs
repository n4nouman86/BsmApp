using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.QAUserPages;

public partial class QALeadEditPage
{
    [Parameter]
    public Guid Id { get; set; }

    [SupplyParameterFromForm]
    private Lead? Input { get; set; }

    private vwLead? Lead { get; set; }  

    private IEnumerable<LeadDispositionStatus>? DispositionStatuses { get; set; }




    protected override async Task OnInitializedAsync()
    {
        using var context = contextFactory.CreateDbContext();

		DispositionStatuses = await context.LeadDispositionStatuses.ToListAsync();

		Input = await context.Leads.FindAsync(Id);

        Lead = await context.vwLeads.FirstOrDefaultAsync(x => x.Id == Id);
    }




    private async Task UpdateLead()
    {
        using var context = contextFactory.CreateDbContext();
        if (Input is null) return;

        Input.IsBillable = true;

        if (Input.DispositionStatusId == 2)
        {
            Input.IsBillable = false;
        }

        context.Entry(Input).State = EntityState.Modified;
        await context.SaveChangesAsync();

        navManager.NavigateTo("QALeadIndex");
    }
}