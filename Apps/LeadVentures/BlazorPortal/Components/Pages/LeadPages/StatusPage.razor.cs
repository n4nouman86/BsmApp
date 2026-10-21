using BlazorPortal.Data;
using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Humanizer;

namespace BlazorPortal.Components.Pages.LeadPages;

public partial class StatusPage : IDisposable
{
    [Parameter]
    public Guid Id { get; set; }

    private vwLead? Lead { get; set; }
    private LeadComment InputComment { get; set; } = new LeadComment();
    private List<vwLeadComment>? LeadComments { get; set; }
    private ApplicationUser? AppUser { get; set; }




    protected override async Task OnInitializedAsync()
    {
        var authState = await authProvider.GetAuthenticationStateAsync();
        AppUser = await userManager.GetUserAsync(authState.User);

        await using var db = await DbFactory.CreateDbContextAsync();
        Lead = await db.vwLeads.FirstOrDefaultAsync(x => x.Id == Id);

        LeadComments = await db.vwLeadComments
           .Where(x => x.LeadId == Id)
           .ToListAsync();

        ResetCommentForm();

        await InitHubConnection();
    }




    private HubConnection? connection;
    private async Task InitHubConnection()
    {
		string url = navManager.ToAbsoluteUri("/leadcommenthub").ToString();

		connection = new HubConnectionBuilder()
            .WithUrl(url)
			.WithAutomaticReconnect()
            .WithStatefulReconnect()
            .Build();

        await connection.StartAsync();
        await connection.SendAsync("AddToGroup", Id);

        connection.On<vwLeadComment>("ReceiveComment", async (comment) =>
        {
            if (LeadComments is null)
            {
                LeadComments = new List<vwLeadComment>();
            }

            LeadComments.Add(comment);
            await InvokeAsync(StateHasChanged);
        });

    }




    private void ResetCommentForm()
    {
        InputComment = new LeadComment();
        InputComment.LeadId = Id;
        InputComment.AppUserId = AppUser!.Id;
    }




    private async Task PostComment()
    {
        InputComment.CommentOn = DateTimeOffset.Now;

        await using var db = await DbFactory.CreateDbContextAsync();
        var authState = await authProvider.GetAuthenticationStateAsync();
        db.CurrentUserName = authState.User.Identity?.Name ?? "Anonymous";
        await db.LeadComments.AddAsync(InputComment);
        await db.SaveChangesAsync();

        if (LeadComments is null)
        {
            LeadComments = new List<vwLeadComment>();
        }
        
        vwLeadComment comment = new vwLeadComment 
        { 
            Id = InputComment.Id, 
            LeadId = InputComment.LeadId, 
            AppUserId = InputComment.AppUserId,
            FullName = AppUser?.FullName,
            CommentOn = InputComment.CommentOn,
            Comment = InputComment.Comment
        };

        connection?.SendAsync("SendComment", Id, comment);

        await Task.Run(CreateNotifications);
        
        ResetCommentForm();
    }




    private async Task CreateNotifications()
    {
        await using var db = await DbFactory.CreateDbContextAsync();

        string notiText = $"New comment on lead {Lead!.FullName} by {AppUser!.FullName}: {InputComment.Comment.Truncate(125)}";

		FormattableString sql = $@"EXEC spCreateNotification {Lead!.Id}, {AppUser!.Id}, {Lead!.AgentId}, {notiText}";
        await db.Database.ExecuteSqlAsync(sql);
    }
    



    public void Dispose()
    {
        if (connection is not null)
        {
            connection.DisposeAsync();
        }
    }
}