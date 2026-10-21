using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace BlazorPortal.Components.Pages.PerformanceLinePages;

public partial class IndexPage
{
    [Parameter] 
    public int PerformanceId { get; set; }

    private List<PerformanceLine>? Lines { get; set; }
    private DateTime PerformanceDate { get; set; }




    protected override async Task OnInitializedAsync()
    {
		using var context = contextFactory.CreateDbContext();

		var performance = await context.Performances.FindAsync(PerformanceId);
		if (performance is not null)
			PerformanceDate = performance.PerformancDate;

		Lines = await context.PerformanceLines
			.Where(x => x.PerformanceId == PerformanceId)
			.OrderBy(x => x.TransferTime)
			.ToListAsync();
	}





    private async Task OpenCreate()
    {
        var result = await dialogService.OpenSideAsync<CreatePage>(
            "Add Line",
            new Dictionary<string, object?> { { "PerformanceId", PerformanceId } },
            new SideDialogOptions
            {
                Position = DialogPosition.Right,
                ShowMask = true,
                MinWidth = 350
            });

        if (result is PerformanceLine created)
        {
            Lines?.Insert(0, created);
            Lines = Lines?.OrderBy(x => x.TransferTime).ToList();
        }
    }




    private async Task OpenEdit(PerformanceLine line)
    {
        var result = await dialogService.OpenSideAsync<EditPage>(
            "Edit Line",
            new Dictionary<string, object?> { { "ExistingLine", line } },
            new SideDialogOptions
            {
                Position = DialogPosition.Right,
                ShowMask = true,
                MinWidth = 350
            });

        if (result is PerformanceLine updated)
        {
            var index = Lines?.FindIndex(x => x.Id == updated.Id) ?? -1;
            if (index >= 0)
            {
                Lines![index] = updated;
                Lines = Lines?.OrderBy(x => x.TransferTime).ToList();
            }
        }
    }




    private async Task ConfirmDelete(int id)
    {
        var confirmed = await dialogService.Confirm(
            "Are you sure you want to delete this line?",
            "Delete Line",
            new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

        if (confirmed != true) return;

        using var context = contextFactory.CreateDbContext();
        var record = await context.PerformanceLines.FindAsync(id);
        if (record is null) return;

        context.PerformanceLines.Remove(record);
        await context.SaveChangesAsync();

        Lines?.RemoveAll(x => x.Id == id);
        Lines = new List<PerformanceLine>(Lines!);
    }
}
