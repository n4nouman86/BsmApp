using BlazorPortal.Entities;

namespace BlazorPortal.Components.Pages.DncLeadPages;

public partial class CreatePage
{
    private DncLead Input { get; set; } = new();

    private async Task Save()
    {
        using var context = contextFactory.CreateDbContext();

        Input.CreatedOn = DateTime.Now;
        await context.DncLeads.AddAsync(Input);
        await context.SaveChangesAsync();

        DialogService.CloseSide(Input.Id);
    }

    private void Cancel() => DialogService.CloseSide(false);
}
