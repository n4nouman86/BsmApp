using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.PerformancePages;

public partial class EditPage
{
    [Parameter] 
    public int Id { get; set; } = default!;

    private Performance Input { get; set; } = new();
    private string? ErrorMessage { get; set; }




    protected override async Task OnInitializedAsync()
    {
		using var context = contextFactory.CreateDbContext();
		
        Input = await context.Performances.FindAsync(Id);
    }




    private async Task Update()
    {
        using var context = contextFactory.CreateDbContext();

        var dateOnly = Input.PerformancDate.Date;
        bool exists = await context.Performances.AnyAsync(x => x.PerformancDate == dateOnly && x.Id != Input.Id);

        if (exists)
        {
            ErrorMessage = "A performance record for this date already exists.";
            return;
        }

        Input.PerformancDate = dateOnly;
        context.Entry(Input).State = EntityState.Modified;
        await context.SaveChangesAsync();

        DialogService.CloseSide(Input);
    }




    private void Cancel() => DialogService.CloseSide(false);
}
