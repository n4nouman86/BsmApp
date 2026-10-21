using BlazorPortal.Entities;
using BlazorPortal.MethodExtensions;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.AgentPages;

public partial class EditPage
{
    [Parameter]
    public Guid Id { get; set; }
    private Agent? Input { get; set; } = new Agent();
    private List<string> CampaignStatuses { get; set; } = new List<string> { "Active", "Hold", "Teminated", "Ended" };
	private List<Employee>? Employees { get; set; }



	protected override async Task OnInitializedAsync()
    {
        await using var context = await DbFactory.CreateDbContextAsync();
		
        Employees = await context.Employees.Where(x => x.IsActive).ToListAsync();
		Input = await context.Agents.FindAsync(Id);

        ServerDatetimeOffset();
    }



    private void ServerDatetimeOffset()
    {
        Input!.MonScheduleFrom = Input.MonScheduleFrom?.ToServerDateTimeOffset();
        Input!.TueScheduleFrom = Input.TueScheduleFrom?.ToServerDateTimeOffset();
        Input!.WedScheduleFrom = Input.WedScheduleFrom?.ToServerDateTimeOffset();
        Input!.ThuScheduleFrom = Input.ThuScheduleFrom?.ToServerDateTimeOffset();
        Input!.FriScheduleFrom = Input.FriScheduleFrom?.ToServerDateTimeOffset();
        Input!.SatScheduleFrom = Input.SatScheduleFrom?.ToServerDateTimeOffset();
        Input!.SunScheduleFrom = Input.SunScheduleFrom?.ToServerDateTimeOffset();
            
        Input!.MonScheduleTo = Input.MonScheduleTo?.ToServerDateTimeOffset();
        Input!.TueScheduleTo = Input.TueScheduleTo?.ToServerDateTimeOffset();
        Input!.WedScheduleTo = Input.WedScheduleTo?.ToServerDateTimeOffset();
        Input!.ThuScheduleTo = Input.ThuScheduleTo?.ToServerDateTimeOffset();
        Input!.FriScheduleTo = Input.FriScheduleTo?.ToServerDateTimeOffset();
        Input!.SatScheduleTo = Input.SatScheduleTo?.ToServerDateTimeOffset();
        Input!.SunScheduleTo = Input.SunScheduleTo?.ToServerDateTimeOffset();
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




    private async Task UpdateAgent()
    {
        if (Input is null) return;

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

        await using var db = await DbFactory.CreateDbContextAsync();
        db.Entry(Input).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await db.SaveChangesAsync();

        ServerDatetimeOffset();

        notify.Notify(Radzen.NotificationSeverity.Success, "Agent", "Updated successfully!");
    }
}