using BlazorPortal.Entities;
using BlazorPortal.MethodExtensions;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.AgentUserPages;

public partial class AgentSettingPage
{
    [SupplyParameterFromForm]
    private Agent? Input { get; set; }
    private string ClientZoneInfoId { get; set; } = string.Empty;





    protected override async Task OnInitializedAsync()
    {
        var authState = await authProvider.GetAuthenticationStateAsync();
        var user = await userManager.GetUserAsync(authState.User);

        ClientZoneInfoId = user!.TimeZoneInfoId;

        await using var db = await DbFactory.CreateDbContextAsync();
        Input = await db.Agents.FirstOrDefaultAsync(x => x.AppUserId == user!.Id);

        LocalDatetimeOffset();
    }






    private void LocalDatetimeOffset()
    {
        Input!.MonScheduleFrom = Input.MonScheduleFrom?.ToLacalDateTimeOffset(ClientZoneInfoId);
        Input!.TueScheduleFrom = Input.TueScheduleFrom?.ToLacalDateTimeOffset(ClientZoneInfoId);
        Input!.WedScheduleFrom = Input.WedScheduleFrom?.ToLacalDateTimeOffset(ClientZoneInfoId);
        Input!.ThuScheduleFrom = Input.ThuScheduleFrom?.ToLacalDateTimeOffset(ClientZoneInfoId);
        Input!.FriScheduleFrom = Input.FriScheduleFrom?.ToLacalDateTimeOffset(ClientZoneInfoId);
        Input!.SatScheduleFrom = Input.SatScheduleFrom?.ToLacalDateTimeOffset(ClientZoneInfoId);
        Input!.SunScheduleFrom = Input.SunScheduleFrom?.ToLacalDateTimeOffset(ClientZoneInfoId);

        Input!.MonScheduleTo = Input.MonScheduleTo?.ToLacalDateTimeOffset(ClientZoneInfoId);
        Input!.TueScheduleTo = Input.TueScheduleTo?.ToLacalDateTimeOffset(ClientZoneInfoId);
        Input!.WedScheduleTo = Input.WedScheduleTo?.ToLacalDateTimeOffset(ClientZoneInfoId);
        Input!.ThuScheduleTo = Input.ThuScheduleTo?.ToLacalDateTimeOffset(ClientZoneInfoId);
        Input!.FriScheduleTo = Input.FriScheduleTo?.ToLacalDateTimeOffset(ClientZoneInfoId);
        Input!.SatScheduleTo = Input.SatScheduleTo?.ToLacalDateTimeOffset(ClientZoneInfoId);
        Input!.SunScheduleTo = Input.SunScheduleTo?.ToLacalDateTimeOffset(ClientZoneInfoId);
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
                date.Value.ToLacalDateTimeOffset(ClientZoneInfoId).Offset);
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
        db.Entry(Input).State = EntityState.Modified;
        await db.SaveChangesAsync();

        LocalDatetimeOffset();

        notify.Notify(Radzen.NotificationSeverity.Success, "Agent", "Updated successfully!");
    }
}