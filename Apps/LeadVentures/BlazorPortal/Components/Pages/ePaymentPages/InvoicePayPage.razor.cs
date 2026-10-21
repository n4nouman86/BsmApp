using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace BlazorPortal.Components.Pages.ePaymentPages;

public partial class InvoicePayPage
{
	[Parameter]
	public Guid Id { get; set; }
	public string? ErrorMessage { get; set; }
	private vwInvoice? Invoice { get; set; }




	protected override async Task OnInitializedAsync()
	{
		using var context = contextFactory.CreateDbContext();
		Invoice = await context.vwInvoices
		.FirstOrDefaultAsync(x => x.Id == Id);
	}




	private async Task ProceedToPayment()
	{
		if (Invoice is null)
		{
			return;
		}

		ErrorMessage = null;

		var sessionId = await mastercardService.CreateSession(
			Invoice.Id.ToString(), 
			Invoice.TotalCost.ToString(), 
			$"Payment for invoice no {Invoice.InvoiceNo}",
			navigationManager.ToAbsoluteUri(string.Concat("InvoiceEResponse/", Invoice.Id.ToString())).ToString());

		if (string.IsNullOrEmpty(sessionId))
		{
			ErrorMessage = "Payment can not be initiate. Please contact for more detail.";
		}
		else
		{
			await jsRuntime.InvokeVoidAsync("startCheckout", sessionId);
		}
	}
}