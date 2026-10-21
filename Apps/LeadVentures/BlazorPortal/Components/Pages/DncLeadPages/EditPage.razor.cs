using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.DncLeadPages;

public partial class EditPage
{
    [Parameter] public int DncLeadId { get; set; }

    private DncLead? Input { get; set; }

    protected override async Task OnInitializedAsync()
    {
        using var context = contextFactory.CreateDbContext();
        Input = await context.DncLeads.FindAsync(DncLeadId);
    }

    private async Task Save()
    {
        if (Input is null) return;

        using var context = contextFactory.CreateDbContext();
        context.Entry(Input).State = EntityState.Modified;
        await context.SaveChangesAsync();
        DialogService.CloseSide(Input.Id);
    }

    private void Cancel() => DialogService.CloseSide(false);
}
