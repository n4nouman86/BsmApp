using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorPortal.Components.Pages.ePaymentPages;

public partial class PaymentRequestPage
{
	[Parameter]
	public Guid Id { get; set; }
	private string? ErrorMessage { get; set; }
	private PaymentRequest? Invoice { get; set; }
	private PaymentRequestFrom? From { get; set; }




	protected override async Task OnInitializedAsync()
	{
		using var context = contextFactory.CreateDbContext();
		Invoice = await context.PaymentRequests.FindAsync(Id);

		if (Invoice is null)
		{
			ErrorMessage = "Invoice not found.";
			return;
		}

		From = await context.PaymentRequestFroms.FindAsync(Invoice.FromId);
	}




	private async Task ProceedToPayment()
	{
		if (Invoice is null)
		{
			return;
		}

		ErrorMessage = null;

		var returnUrl = navigationManager
		.ToAbsoluteUri(string.Concat("PaymentRequestResponse/", Invoice.Id.ToString()))
		.ToString();

		var sessionId = await mastercardService
		.CreateSession(
			Invoice.Id.ToString(), 
			Invoice.Amount.ToString(), 
			$"{Invoice.Description}",
			returnUrl);

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