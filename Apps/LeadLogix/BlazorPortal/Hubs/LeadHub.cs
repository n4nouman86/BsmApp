using BlazorPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BlazorPortal.Hubs;


public class LeadHub : Hub
{
    public async Task TransferLead(LeadTransferModel model)
    {
        await Clients.All.SendAsync("ReceiveLead", model);
    }
}
