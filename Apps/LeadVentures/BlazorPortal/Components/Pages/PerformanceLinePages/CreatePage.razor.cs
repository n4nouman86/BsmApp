using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;

namespace BlazorPortal.Components.Pages.PerformanceLinePages;

public partial class CreatePage
{
    [Parameter] public int PerformanceId { get; set; }

    private PerformanceLine Input { get; set; } = new();




    protected override void OnInitialized()
    {
        Input.PerformanceId = PerformanceId;
        Input.TransferTime = DateTime.Now;
    }




    private async Task Save()
    {
        using var context = contextFactory.CreateDbContext();
        await context.PerformanceLines.AddAsync(Input);
        await context.SaveChangesAsync();

        DialogService.CloseSide(Input);
    }




    private void Cancel() => DialogService.CloseSide(false);
}
