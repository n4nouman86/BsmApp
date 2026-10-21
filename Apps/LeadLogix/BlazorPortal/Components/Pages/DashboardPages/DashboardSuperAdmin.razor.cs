using BlazorPortal.Entities;
using BlazorPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.DashboardPages;

public partial class DashboardSuperAdmin
{
    private IEnumerable<vwcTotalDayTransfer>? ChartData { get; set; }




    protected override async Task OnInitializedAsync()
    {
        await using var context = await DbFactory.CreateDbContextAsync();

        var chartTask = Task.Run(async () =>
        {
            ChartData = await context
            .vwcTotalDayTransfers
            .OrderByDescending(x => x.TransferDate)
            .Take(7)
            .ToListAsync();
        });

        var dailyReportTask = Task.Run(async () =>
		{
			await ChangeDailyReportDate();
		});

        var weeklyReportTask = Task.Run(async () =>
        {
			await GetWeeklyTransfer();
		});

		await Task.WhenAll(chartTask, dailyReportTask, weeklyReportTask);

		SetWeeklyReportDates();
	}



	private void SetWeeklyReportDates()
	{
		var date = DateTime.Now;

		while (true)
		{
			if (date.DayOfWeek == DayOfWeek.Monday)
			{
				dfrom = DateOnly.FromDateTime(date);
				dto = dfrom.AddDays(6);
				break;
			}

			date = date.AddDays(-1);
		}
	}



	private DateTime DailyReportDate { get; set; } = DateTimeOffset.Now.ToOffset(new TimeSpan(-5, 0, 0)).DateTime;
    private IEnumerable<spTransferDailyReport>? DailyReports { get; set; }
    private async Task ChangeDailyReportDate()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        FormattableString sql = @$" EXEC spTransferDailyReport {DailyReportDate.Date} ";
        DailyReports = await db.Database.SqlQuery<spTransferDailyReport>(sql).ToListAsync();
    }




    private async Task AddDailyReportDate(int day)
    {
        DailyReportDate = DailyReportDate.AddDays(day);
        await ChangeDailyReportDate();
    }




    private DateOnly dfrom { get; set; } = DateOnly.FromDateTime(DateTimeOffset.Now.ToOffset(new TimeSpan(-5, 0, 0)).DateTime);
    private DateOnly dto { get; set; } = DateOnly.FromDateTime(DateTimeOffset.Now.ToOffset(new TimeSpan(-5, 0, 0)).DateTime);
    private IEnumerable<SpTransferWeeklyReport>? WeeklyReports { get; set; }
    private async Task GetWeeklyTransfer()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        FormattableString sql = @$" Exec dbo.spGetTransferWeeklyReport {dfrom}, {dto} ";
        WeeklyReports = await db.Database.SqlQuery<SpTransferWeeklyReport>(sql).ToListAsync();
    }




    private async Task AddWeeklyReportDates(int days)
    {
        dfrom = dfrom.AddDays(days);
        dto = dto.AddDays(days);

        await GetWeeklyTransfer();
    }
}