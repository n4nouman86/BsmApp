using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace BlazorPortal.Components.Pages.PerformancePages;

public partial class IndexPage
{
    private List<Performance>? Performances { get; set; }




    protected override async Task OnInitializedAsync()
    {
		using var context = contextFactory.CreateDbContext();

		Performances = await context.Performances
			.OrderByDescending(x => x.PerformancDate)
			.Take(12)
			.ToListAsync();
	}





    private async Task OpenCreate()
    {
        var result = await dialogService.OpenSideAsync<CreatePage>(
            "Add Performance",
            options: new SideDialogOptions
            {
                Position = DialogPosition.Right,
                ShowMask = true,
                MinWidth = 400,
                Width = "550px"
            });

        if (result is Performance created)
        {
            Performances?.Insert(0, created);
            Performances = new List<Performance>(Performances!);
        }
    }




    private async Task OpenEdit(Performance performance)
    {
        var result = await dialogService.OpenSideAsync<EditPage>(
            "Edit Performance",
            new Dictionary<string, object?> { { "Id", performance.Id } },
            new SideDialogOptions
            {
                Position = DialogPosition.Right,
                ShowMask = true,
                MinWidth = 400,
				Width = "550px"
			});

        if (result is Performance updated)
        {
            var index = Performances?.FindIndex(x => x.Id == updated.Id) ?? -1;
            if (index >= 0)
            {
                Performances![index] = updated;
                Performances = new List<Performance>(Performances.OrderByDescending(x => x.PerformancDate));
            }
        }
    }




    private async Task ConfirmDelete(int id)
    {
        var confirmed = await dialogService.Confirm(
            "Are you sure you want to delete this performance record?",
            "Delete Performance",
            new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

        if (confirmed != true) return;

        using var context = contextFactory.CreateDbContext();

        var lines = await context.PerformanceLines.Where(x => x.PerformanceId == id).ToListAsync();
        
        if (lines.Any())
        {
            context.RemoveRange(lines);
        }

        var record = await context.Performances.FindAsync(id);
        if (record is null) return;

        context.Performances.Remove(record);
        await context.SaveChangesAsync();

        Performances?.RemoveAll(x => x.Id == id);
        Performances = new List<Performance>(Performances!);
    }
}
