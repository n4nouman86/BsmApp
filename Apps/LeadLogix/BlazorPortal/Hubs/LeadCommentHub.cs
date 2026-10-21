using BlazorPortal.Entities;
using Microsoft.AspNetCore.SignalR;

namespace BlazorPortal.Hubs;

public class LeadCommentHub : Hub
{
    public async Task AddToGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }






    public async Task SendComment(string groupName, vwLeadComment comment)
    {
        await Clients.Group(groupName).SendAsync("ReceiveComment", comment);
    }
}
