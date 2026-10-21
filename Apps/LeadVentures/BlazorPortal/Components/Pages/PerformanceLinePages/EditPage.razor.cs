using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.PerformanceLinePages;

public partial class EditPage
{
    [Parameter] public PerformanceLine ExistingLine { get; set; } = default!;

    private PerformanceLine Input { get; set; } = new();




    protected override void OnInitialized()
    {
        Input = new PerformanceLine
        {
            Id = ExistingLine.Id,
            PerformanceId = ExistingLine.PerformanceId,
            TransferTime = ExistingLine.TransferTime,
            KgBestDay = ExistingLine.KgBestDay,
            KgTransfers = ExistingLine.KgTransfers,
            BsmBestDay = ExistingLine.BsmBestDay,
            BsmTransfers = ExistingLine.BsmTransfers,
        };
    }




    private async Task Save()
    {
        using var context = contextFactory.CreateDbContext();
        context.Entry(Input).State = EntityState.Modified;
        await context.SaveChangesAsync();

        DialogService.CloseSide(Input);
    }




    private void Cancel() => DialogService.CloseSide(false);
}
