using BlazorPortal.Entities;
using BlazorPortal.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Radzen;
using System.Security.Claims;

namespace BlazorPortal.Components.Pages.LeadPages;

public partial class TransferPage : IAsyncDisposable
{
    private Lead Input { get; set; } = new Lead();
	private List<vwdActiveAgent>? ActiveAgents { get; set; }
	private IEnumerable<vwState>? States { get; set; }
	private string AppUserId { get; set; } = string.Empty;
    private string TcpaMessage { get; set; } = string.Empty;




	protected override async Task OnInitializedAsync()
    {
        await using var db = await DbFactory.CreateDbContextAsync();

        ActiveAgents = await db.vwdActiveAgents.ToListAsync();
        States = await db.vwStates.ToListAsync();

        var authState = await authProvider.GetAuthenticationStateAsync();
        AppUserId = authState.User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        await InitConnection();
    }





    bool isAgentRefereshing = false;
    private async Task RefereshAgents()
    {
        isAgentRefereshing = true;
        await InvokeAsync(StateHasChanged);

        await using var db = await DbFactory.CreateDbContextAsync();
        ActiveAgents = await db.vwdActiveAgents.ToListAsync();
        notify.Notify(NotificationSeverity.Success, "Success", "Agents list refreshed.");

        isAgentRefereshing = false;
    }




    private IEnumerable<vwLead>? TransferLeads { get; set; }
    private async Task CheckTransfer()
    {
        TransferLeads = null;
        TcpaMessage = string.Empty;
        bool canTransfer = true;

		if (string.IsNullOrWhiteSpace(Input.PhoneNumber))
        {
            return;
        }

		await using var db = await DbFactory.CreateDbContextAsync();

		var lead1s = await db.vwLeads
            .Where(x => x.PhoneNumber == Input.PhoneNumber)
            .Select(x => new vwLead { TransferOn = x.TransferOn, AgentFullName = $"{x.AgentFullName} (Logix)" })
            .ToListAsync();

		await using var ventureDb = await VentureDbFactory.CreateDbContextAsync();

		var lead2s = await ventureDb.vwLeads
	        .Where(x => x.PhoneNumber == Input.PhoneNumber)
	        .Select(x => new vwLead { TransferOn = x.TransferOn, AgentFullName = $"{x.AgentFullName} (Venture)" })
	        .ToListAsync();

        var dncs = await ventureDb.DncLeads
			.Where(x => x.Phone == Input.PhoneNumber)
            .Select(x => new vwLead { TransferOn = DateTime.Now.AddHours(-2), AgentFullName = $"{x.Phone} (DNC)" })
			.ToListAsync();

		TransferLeads = lead1s.Concat(lead2s).Concat(dncs).ToList();
        TransferLeads = TransferLeads.OrderByDescending(x => x.TransferOn).ToList();

		if (TransferLeads.Any())
        {
            var setting = await db.LeadSettings.FirstOrDefaultAsync() ?? new LeadSetting() { CheckTransferDays = 90 };

			if (TransferLeads.First().TransferOn >= DateTime.Now.AddDays(-setting.CheckTransferDays))
			{
				canTransfer = false;
			}
		}

        TcpaMessage = await tcpaService.GetDncStatus(Input.PhoneNumber);

		if (TcpaMessage != "clean")
		{
			canTransfer = false;
		}

		if (!canTransfer)
		{
			await dialog.Alert(
	            "You can not transfer this number.",
	            "Warning");

			Input.PhoneNumber = string.Empty;
		}
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

        await hubConnection.StartAsync();
    }




    private Agent? Agent { get; set; }
    private async Task ChangedAgent()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        Agent = await db.Agents.FindAsync(Input.AgentId);

        if (Agent is null)
        {
            Input.CustomLabel1 = null;
            Input.CustomLabel2 = null;
            Input.CustomLabel3 = null;
            Input.CustomLabel4 = null;
            Input.CustomLabel5 = null;
            Input.CustomLabel6 = null;
            Input.CustomLabel7 = null;
            Input.CustomLabel8 = null;
            Input.CustomLabel9 = null;
            Input.CustomLabel10 = null;
        }
        else
        {
            Input.CustomLabel1 = Agent.CustomLabel1;
            Input.CustomLabel2 = Agent.CustomLabel2;
            Input.CustomLabel3 = Agent.CustomLabel3;
            Input.CustomLabel4 = Agent.CustomLabel4;
            Input.CustomLabel5 = Agent.CustomLabel5;
            Input.CustomLabel6 = Agent.CustomLabel6;
            Input.CustomLabel7 = Agent.CustomLabel7;
            Input.CustomLabel8 = Agent.CustomLabel8;
            Input.CustomLabel9 = Agent.CustomLabel9;
            Input.CustomLabel10 = Agent.CustomLabel10;

            var st = States?.FirstOrDefault(x => x.Text.Contains(Agent.States));

            if (st is not null)
            {
                Input.StateId = st.Id;
            }
        }
    }




    bool isTransfering = false;
    private async Task TransferLead()
    {
        isTransfering = true;

        Input.Id = Guid.NewGuid();
        Input.TransferOn = DateTime.Now;
        Input.IsBillable = true;
        Input.VerifierId = AppUserId;

        await using var db = await DbFactory.CreateDbContextAsync();
        var authState = await authProvider.GetAuthenticationStateAsync();
        db.CurrentUserName = authState.User.Identity?.Name ?? "Anonymous";
        await db.Leads.AddAsync(Input);
        await db.SaveChangesAsync();
        Agent = null;

        if (hubConnection is not null)
        {
            var ltm = new LeadTransferModel { AgentId = Input.AgentId, LeadId = Input.Id, Phone = Input.PhoneNumber, Name = Input.FullName, Address = Input.Address };
            await hubConnection.SendAsync("TransferLead", ltm);
        }

        ResetForm();

		notify.Notify(NotificationSeverity.Success, "Success", "Lead transfer successfully!");
        
        isTransfering = false;
    }




    private void ResetForm()
    {
        Input = new Lead();
        Agent = null;
		TransferLeads = null;
		TcpaMessage = string.Empty;

		notify.Notify(NotificationSeverity.Success, "Success", "Form has been reset.");
    }




    private string CalledLeadStatus { get; set; } = "Call Back";
	private async Task SaveLead()
    {
		if (string.IsNullOrWhiteSpace(Input.PhoneNumber))
		{
			notify.Notify(NotificationSeverity.Error, "Error", "Phone number is required.");
			return;
		}

		var calledLead = new CalledLead
		{
			VerifierId = AppUserId,
			PhoneNumber = Input.PhoneNumber,
			FullName = Input.FullName,
			DateOfBirth = Input.DateOfBirth,
			Address = Input.Address,
			StateId = Input.StateId == 0 ? null : Input.StateId,
			Spouse = Input.Spouse,
			SpouseName = Input.SpouseName,
			SpouseDob = Input.SpouseDob,
			CurrentAutoCarrier = Input.CurrentAutoCarrier,
			AnyLapses = Input.AnyLapses,
			MakeAndModel = Input.MakeAndModel,
			Accidents = Input.Accidents,
			Tickets = Input.Tickets,
			HomeOwner = Input.HomeOwner,
			HomeType = Input.HomeType,
			HaveHomeClaims = Input.HaveHomeClaims,
			CurrentHomeCarrier = Input.CurrentHomeCarrier,
			AnyClaimInPastThreeYears = Input.AnyClaimInPastThreeYears,
			CalledOn = DateTime.Now,
            Status = CalledLeadStatus
		};

		await using var context = await DbFactory.CreateDbContextAsync();

		await context.CalledLeads.AddAsync(calledLead);
		await context.SaveChangesAsync();

		notify.Notify(NotificationSeverity.Success, "Success", "Lead saved successfully!");

        ResetForm();
	}



    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null) 
        {
            await hubConnection.DisposeAsync();
        }
    }
}