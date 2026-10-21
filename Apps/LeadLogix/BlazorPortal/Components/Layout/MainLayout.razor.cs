using BlazorPortal.Entities;
using BlazorPortal.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Radzen;

namespace BlazorPortal.Components.Layout
{
    public partial class MainLayout : IAsyncDisposable
    {
        private Agent? Agent { get; set; }
        private vwAppUser? AppUser { get; set; }




        protected async override Task OnInitializedAsync()
        {
            var authState = await authProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user is null || user.Identity is null || user.Identity.Name is null)
            {
                throw new Exception("main layout: identity user not found.");
            }

            await using var context = await DbFactory.CreateDbContextAsync();

            AppUser = await context.vwAppUsers.FirstOrDefaultAsync(x => x.Email.ToLower() == user.Identity.Name.ToLower());

            if (AppUser is null)
            {
                throw new Exception("main layout: app user not found.");
            }

            if (user.IsInRole("Agent"))
            {
                Agent = await context.Agents.FirstOrDefaultAsync(x => x.AppUserId == AppUser.Id);

                await InitConnection();
            }

            timer = new Timer(async (arg)=> 
            {
                await UpdateUserStatus();
            }, null, 0, 60000);
        }




        private HubConnection? hubConnection;
        private async Task InitConnection()
        {
			string url = navManager.ToAbsoluteUri("/leadhub").ToString();

			hubConnection = new HubConnectionBuilder()
                .WithUrl(url)
				.WithAutomaticReconnect()
                .WithStatefulReconnect()
                .Build();

            hubConnection.On<LeadTransferModel>("ReceiveLead", async (model) =>
            {
                if (model.AgentId != Agent!.Id) return;

                notify.Notify(
                    NotificationSeverity.Info,
                    "New Lead",
                    $"Phone: {model.Phone}",
                    int.MaxValue,
                    (nm) => { navManager.NavigateTo(String.Concat("AgentLeadOpen/", model.LeadId)); },
                    false,
                    DateTime.Now,
                    null);

                await js.InvokeVoidAsync("playTransferSound");
            });

            await hubConnection.StartAsync();
        }




        private Timer? timer { get; set; }
        private async Task UpdateUserStatus()
        {
            if (AppUser is not null)
            {
                FormattableString sql = $@"UPDATE AspNetUsers
                                SET
	                                LastOnlineOn = {DateTimeOffset.Now}
                                WHERE
	                                Id = {AppUser.Id}";

                await using var context = await DbFactory.CreateDbContextAsync();
                
                await context.Database.ExecuteSqlAsync(sql);
            }
        }




        public async ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
            }

            if (timer is not null) 
            {
                await timer.DisposeAsync();
            }
        }
    }
}