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
        using var context = contextFactory.CreateDbContext();
        ActiveAgents = await context.vwdActiveAgents.ToListAsync();
        States = await context.vwStates.ToListAsync();

        var authState = await authProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        
        AppUserId = authState.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        await InitConnection();
    }





    bool isAgentRefereshing = false;
    private async Task RefereshAgents()
    {
        isAgentRefereshing = true;
        await InvokeAsync(StateHasChanged);

        using var context = contextFactory.CreateDbContext();
        ActiveAgents = await context.vwdActiveAgents.ToListAsync();
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

		using var context = contextFactory.CreateDbContext();

		var lead1s = await context.vwLeads
			.Where(x => x.PhoneNumber == Input.PhoneNumber)
			.Select(x => new vwLead { TransferOn = x.TransferOn, AgentFullName = $"{x.AgentFullName} (Venture)" })
			.ToListAsync();

		using var logixContext = logixContextFactory.CreateDbContext();

		var lead2s = await logixContext.vwLeads
			.Where(x => x.PhoneNumber == Input.PhoneNumber)
			.Select(x => new vwLead { TransferOn = x.TransferOn, AgentFullName = $"{x.AgentFullName} (Logix)" })
			.ToListAsync();

        var dncs = await context.DncLeads
        .Where(x => x.Phone == Input.PhoneNumber)
        .Select(x => new vwLead { TransferOn = DateTime.Now.AddHours(-2), AgentFullName = $"{x.Phone} (DNC)" })
        .ToListAsync();

		TransferLeads = lead1s.Concat(lead2s).Concat(dncs).ToList();
		TransferLeads = TransferLeads.OrderByDescending(x => x.TransferOn).ToList();

		if (TransferLeads.Any())
		{
			var setting = await context.LeadSettings.FirstOrDefaultAsync() ?? new LeadSetting{ CheckTransferDays = 90 };

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
        using var context = contextFactory.CreateDbContext();
        Agent = await context.Agents.FindAsync(Input.AgentId);

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
        await InvokeAsync(StateHasChanged);

        using var context = contextFactory.CreateDbContext();
        Input.Id = Guid.NewGuid();
		Input.TransferOn = DateTime.Now;
        Input.IsBillable = true;
        Input.VerifierId = AppUserId;

        await context.Leads.AddAsync(Input);
        await context.SaveChangesAsync();

        if (Input.AgentId == Guid.Parse("721d9ecf-8987-4e7e-9186-c22c9922fad8"))
        {
            await oscarService.CreateLeadAsync(Input, States?.FirstOrDefault(x => x.Id == Input.StateId)?.Text);
        }

        if (Input.AgentId == Guid.Parse("38c67697-77aa-4e5b-a9e7-ef5061610b50"))
        {
            await miretService.CreateLeadAsync(Input, States?.FirstOrDefault(x => x.Id == Input.StateId)?.Text);
        }

        if (Input.AgentId == Guid.Parse("7e42f444-c061-45c7-8cac-adbff657b811"))
        {
            await estersonService.CreateLeadAsync(Input, States?.FirstOrDefault(x => x.Id == Input.StateId)?.Text);
        }

        if (Input.AgentId == Guid.Parse("e4c68b11-5fe6-449c-9d0e-5ef7788c841f"))
        {
            await mikeService.CreateLeadAsync(Input, States?.FirstOrDefault(x => x.Id == Input.StateId)?.Text);
        }

        //if (Input.AgentId == Guid.Parse("bbd7bd24-cac5-4e24-ae29-7fe09e9649f1"))
        //{
        //    await wyattService.CreateLeadAsync(Input, States?.FirstOrDefault(x => x.Id == Input.StateId)?.Text);
        //}

        if (Input.AgentId == Guid.Parse("87df31b2-bfbe-4282-b2e1-aa085285f7f3"))
        {
            await markMercerService.CreateLeadAsync(Input, States?.FirstOrDefault(x => x.Id == Input.StateId)?.Text);
        }

		if (Input.AgentId == Guid.Parse("04097791-4154-407f-b792-7781f5e99491"))
		{
			await jayDepoloService.CreateLeadAsync(Input, States?.FirstOrDefault(x => x.Id == Input.StateId)?.Text);
		}

        if (Input.AgentId == Guid.Parse("75e70cf2-66c4-47a6-a3ab-f02ce589b0af"))
        {
			await adamWingService.CreateLeadAsync(Input, States?.FirstOrDefault(x => x.Id == Input.StateId)?.Text);
		}

		if (Input.AgentId == Guid.Parse("76a94b21-a7ee-4e39-8957-e65e628f0ad8"))
		{
			await lisaFainaService.CreateLeadAsync(Input, States?.FirstOrDefault(x => x.Id == Input.StateId)?.Text);
		}

        if (Input.AgentId == Guid.Parse("89ea3062-a85a-40f9-b767-43e13198a6b0"))
        {
            await lisaFainaService2.CreateLeadAsync(Input, States?.FirstOrDefault(x => x.Id == Input.StateId)?.Text);
        }

		if (Input.AgentId == Guid.Parse("c992832a-0d60-4419-ae29-00403eb09474"))
		{
			await jorgeCarbonellService.CreateLeadAsync(Input, States?.FirstOrDefault(x => x.Id == Input.StateId)?.Text);
		}

		//if (Input.AgentId == Guid.Parse("8583050d-d875-4eb8-bf54-3ddc02ac3caf"))
		//{
		//	await stefanSmallwoodService.CreateLeadAsync(Input, States?.FirstOrDefault(x => x.Id == Input.StateId)?.Text);
		//}

		Agent = null;

        if (hubConnection is not null)
        {
            var ltm = new LeadTransferModel { AgentId = Input.AgentId, LeadId = Input.Id, Phone = Input.PhoneNumber, Name = Input.FullName, Address = Input.Address };
            await hubConnection.SendAsync("TransferLead", ltm);
        }

		notify.Notify(NotificationSeverity.Success, "Success", "Lead transfer successfully!");

		ResetForm();

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

		await using var context = await contextFactory.CreateDbContextAsync();

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