using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;
using Radzen.Blazor.Rendering;

namespace BlazorPortal.Components.Pages.PerformancePages;

public partial class CreatePage
{
    private Performance Input { get; set; } = new() { PerformancDate = DateTime.Today };
    private Performance? LastPerformance { get; set; }
    private string? ErrorMessage { get; set; }



	protected override async Task OnInitializedAsync()
	{
		using var context = contextFactory.CreateDbContext();

        LastPerformance = await context.Performances
            .AsNoTracking()
            .OrderByDescending(x => x.PerformancDate)
            .FirstOrDefaultAsync();

        if (LastPerformance is not null)
        {
            Input.Id = 0;
            Input.PerformancDate = DateTime.Today;
            Input.KGTarget = LastPerformance.KGTarget;
            Input.KGReps = LastPerformance.KGReps;
            Input.KGVerifiers = LastPerformance.KGVerifiers;
            Input.BSMTarget = LastPerformance.BSMTarget;
            Input.BSMReps = LastPerformance.BSMReps;
            Input.BSMVerifiers = LastPerformance.BSMVerifiers;
            Input.KGDialerCost = 0;
            Input.BSMDialerCost = 0;
            Input.DurketaDialerCost = 0;
        }
	}



    private async Task Create()
    {
        using var context = contextFactory.CreateDbContext();

        var dateOnly = Input.PerformancDate.Date;
        bool exists = await context.Performances.AnyAsync(x => x.PerformancDate == dateOnly);

        if (exists)
        {
            ErrorMessage = "A performance record for this date already exists.";
            return;
        }

        Input.PerformancDate = dateOnly;
        await context.Performances.AddAsync(Input);
        await context.SaveChangesAsync();

		if (LastPerformance is not null)
		{
            var lines = await context.PerformanceLines
            .Where(x => x.PerformanceId == LastPerformance.Id)
            .OrderBy(x => x.TransferTime)
            .ToListAsync();
            
            var startDailingDateTime = DateTime.Now;
            
            if(lines.Any())
            {
                PerformanceLine fl = lines.OrderBy(x => x.TransferTime).FirstOrDefault()!;

                startDailingDateTime = new DateTime(
					DateOnly.FromDateTime(Input.PerformancDate),
					new TimeOnly(fl.TransferTime.Hour, fl.TransferTime.Minute));
			}

			List<PerformanceLine> newLines = new();

			foreach (var line in lines)
			{
                var newLine = new PerformanceLine();

                newLine.PerformanceId = Input.Id;

                newLine.TransferTime = startDailingDateTime;

                newLine.KgBestDay = line.KgBestDay;
                newLine.BsmBestDay = line.BsmBestDay;

                newLines.Add(newLine);

                startDailingDateTime = startDailingDateTime.AddMinutes(30);
			}

			if (newLines.Count > 0)
			{
                await context.AddRangeAsync(newLines);
                await context.SaveChangesAsync();
			}
		}

		DialogService.CloseSide(Input);
	}




	private void Cancel() => DialogService.CloseSide(false);
}
