using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace BlazorPortal.Components.Pages.DncLeadPages;

public partial class IndexPage
{
    private List<DncLead>? DncLeads { get; set; }

    protected override async Task OnInitializedAsync()
    {
        using var context = contextFactory.CreateDbContext();
        DncLeads = await context.DncLeads.OrderByDescending(x => x.Id).Take(50).ToListAsync();
    }

    private InputModel Input { get; set; } = new();

    private async Task Search()
    {
        using var context = contextFactory.CreateDbContext();
        var query = context.DncLeads.AsQueryable();

        if (!string.IsNullOrWhiteSpace(Input.Phone))
            query = query.Where(x => x.Phone.Contains(Input.Phone));

        DncLeads = await query.OrderByDescending(x => x.Id).ToListAsync();
    }

    private async Task OpenCreate()
    {
        var result = await dialogService.OpenSideAsync<CreatePage>(
            "Add DNC Lead",
            options: new SideDialogOptions { Position = DialogPosition.Right, ShowMask = true, MinWidth = 450, Width = "600px" });

        if (result is int createdId)
        {
            using var context = contextFactory.CreateDbContext();
            var record = await context.DncLeads.FindAsync(createdId);
            if (record is not null)
            {
                DncLeads?.Insert(0, record);
                DncLeads = new List<DncLead>(DncLeads!);
            }
        }
    }

    private async Task OpenEdit(int id)
    {
        var result = await dialogService.OpenSideAsync<EditPage>(
            "Edit DNC Lead",
            new Dictionary<string, object?> { { "DncLeadId", id } },
            new SideDialogOptions { Position = DialogPosition.Right, ShowMask = true, MinWidth = 450, Width = "600px" });

        if (result is int updatedId)
        {
            using var context = contextFactory.CreateDbContext();
            var record = await context.DncLeads.FindAsync(updatedId);
            if (record is not null)
            {
                var index = DncLeads?.FindIndex(x => x.Id == updatedId) ?? -1;
                if (index >= 0)
                {
                    DncLeads![index] = record;
                    DncLeads = new List<DncLead>(DncLeads);
                }
            }
        }
    }

    private async Task Delete(int id)
    {
        var confirmed = await dialogService.Confirm(
            "Are you sure you want to delete this DNC lead?",
            "Delete Confirmation",
            new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

        if (confirmed is not true) return;

        using var context = contextFactory.CreateDbContext();
        await context.DncLeads.Where(x => x.Id == id).ExecuteDeleteAsync();
        DncLeads = DncLeads?.Where(x => x.Id != id).ToList();
    }

    private sealed class InputModel
    {
        public string Phone { get; set; } = string.Empty;
    }
}
