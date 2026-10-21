using BlazorPortal.Entities;
using BlazorPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.DashboardPages;

public partial class PerforamncePage
{
    private DateTime SelectedDate { get; set; } = DateTime.Today;
    private Performance? CurrentPerformance { get; set; }
    private List<PerformanceRowVM>? Rows { get; set; }

	private int OverallTarget => (CurrentPerformance?.KGTarget ?? 0) + (CurrentPerformance?.BSMTarget ?? 0);
	private int KgAchieved => Rows?.LastOrDefault()?.KgTransfersTillTime ?? 0;
    private int BsmAchieved => Rows?.LastOrDefault()?.BsmTransfersTillTime ?? 0;
    private int KgRem => (CurrentPerformance?.KGTarget ?? 0) - KgAchieved;
    private int BsmRem => (CurrentPerformance?.BSMTarget ?? 0) - BsmAchieved;
    private int TotalAchieved => KgAchieved + BsmAchieved;
    private int TotalRemaining => OverallTarget - TotalAchieved;
    private int TotalReps => (CurrentPerformance?.KGReps ?? 0) + (CurrentPerformance?.BSMReps ?? 0);
    private int TotalVerifiers => (CurrentPerformance?.KGVerifiers ?? 0) + (CurrentPerformance?.BSMVerifiers ?? 0);




    protected override async Task OnInitializedAsync()
    {
        await LoadPerformance();
    }




    private async Task LoadPerformance()
    {
        using var context = contextFactory.CreateDbContext();

        CurrentPerformance = await context.Performances
            .FirstOrDefaultAsync(x => x.PerformancDate == SelectedDate.Date);

        if (CurrentPerformance is null)
        {
            Rows = null;
            return;
        }

        var lines = await context.PerformanceLines
            .Where(x => x.PerformanceId == CurrentPerformance.Id)
            .OrderBy(x => x.TransferTime)
            .ToListAsync();

        Rows = PerformanceRowVM.BuildRows(lines);
    }
}
