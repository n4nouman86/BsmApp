using BlazorPortal.Entities;
using BlazorPortal.MethodExtensions;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.AgentPages;

public partial class CreatePage
{
    private Agent Input { get; set; } = new Agent();

    private List<vwdAgentAppUser>? AgentAppUsers { get; set; }
    private List<string> CampaignStatuses { get; set; } = new List<string> { "Active", "Hold", "Teminated", "Ended" };
	private List<Employee>? Employees { get; set; }




	protected override async Task OnInitializedAsync()
    {
		await using var context = await DbFactory.CreateDbContextAsync();
		Employees = await context.Employees.Where(x => x.IsActive).ToListAsync();

		await LoadFormData();
	}




    private async Task LoadFormData()
    {
		await using var context = await DbFactory.CreateDbContextAsync();
		
        AgentAppUsers = await context.vwdAgentAppUsers
        .OrderByDescending(x => x.Text)
        .ToListAsync();

        Input = new Agent()
        {
			CampaignStatus = "Active",
            MonScheduleFrom = new DateTimeOffset(new DateOnly(2000, 1, 1), new TimeOnly(18,0,0), TimeSpan.Zero),
			MonScheduleTo = new DateTimeOffset(new DateOnly(2000, 1, 1), new TimeOnly(2, 0, 0), TimeSpan.Zero),
			TueScheduleFrom = new DateTimeOffset(new DateOnly(2000, 1, 1), new TimeOnly(18, 0, 0), TimeSpan.Zero),
            TueScheduleTo = new DateTimeOffset(new DateOnly(2000, 1, 1), new TimeOnly(2, 0, 0), TimeSpan.Zero),
			WedScheduleFrom = new DateTimeOffset(new DateOnly(2000, 1, 1), new TimeOnly(18, 0, 0), TimeSpan.Zero),
			WedScheduleTo = new DateTimeOffset(new DateOnly(2000, 1, 1), new TimeOnly(2, 0, 0), TimeSpan.Zero),
			ThuScheduleFrom = new DateTimeOffset(new DateOnly(2000, 1, 1), new TimeOnly(18, 0, 0), TimeSpan.Zero),
			ThuScheduleTo = new DateTimeOffset(new DateOnly(2000, 1, 1), new TimeOnly(2, 0, 0), TimeSpan.Zero),
			FriScheduleFrom = new DateTimeOffset(new DateOnly(2000, 1, 1), new TimeOnly(18, 0, 0), TimeSpan.Zero),
			FriScheduleTo = new DateTimeOffset(new DateOnly(2000, 1, 1), new TimeOnly(2, 0, 0), TimeSpan.Zero),
		};
	}




    private DateTimeOffset? CreateDateTimeOffset(DateTimeOffset? date)
    {
        if (date.HasValue)
        {
            date = new DateTimeOffset(
                2000,
                1,
                1,
                date.Value.Hour,
                date.Value.Minute,
                0,
                date.Value.ToServerDateTimeOffset().Offset);
        }

        return date;
    }




    private async Task CreateAgent()
    {
        Input.Id = Guid.NewGuid();
        Input.CreateOn = DateTime.Now;

        Input.MonScheduleFrom = CreateDateTimeOffset(Input.MonScheduleFrom);
        Input.TueScheduleFrom = CreateDateTimeOffset(Input.TueScheduleFrom);
        Input.WedScheduleFrom = CreateDateTimeOffset(Input.WedScheduleFrom);
        Input.ThuScheduleFrom = CreateDateTimeOffset(Input.ThuScheduleFrom);
        Input.FriScheduleFrom = CreateDateTimeOffset(Input.FriScheduleFrom);
        Input.SatScheduleFrom = CreateDateTimeOffset(Input.SatScheduleFrom);
        Input.SunScheduleFrom = CreateDateTimeOffset(Input.SunScheduleFrom);
        
        Input.MonScheduleTo = CreateDateTimeOffset(Input.MonScheduleTo);
        Input.TueScheduleTo = CreateDateTimeOffset(Input.TueScheduleTo);
        Input.WedScheduleTo = CreateDateTimeOffset(Input.WedScheduleTo);
        Input.ThuScheduleTo = CreateDateTimeOffset(Input.ThuScheduleTo);
        Input.FriScheduleTo = CreateDateTimeOffset(Input.FriScheduleTo);
        Input.SatScheduleTo = CreateDateTimeOffset(Input.SatScheduleTo);
        Input.SunScheduleTo = CreateDateTimeOffset(Input.SunScheduleTo);

        await using var context = await DbFactory.CreateDbContextAsync();

        await context.Agents.AddAsync(Input);
        await context.SaveChangesAsync();

        await LoadFormData();

		notify.Notify(Radzen.NotificationSeverity.Success, "Agent", "Created successfully!");
    }
}