using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace BlazorPortal.Components.Pages.ePaymentPages;

public partial class PaymentRequestResponsePage
{
	[Parameter]
	public Guid Id { get; set; }
	private string? ErrorMessage { get; set; }
	private string? SuccessMessage { get; set; }
	private PaymentRequest? Request { get; set; }
	private PaymentRequestFrom? From { get; set; }




	protected override async Task OnInitializedAsync()
	{
		using var context = contextFactory.CreateDbContext();
		Request = await context.PaymentRequests.FindAsync(Id);
		
		if (Request is null)
		{
			ErrorMessage = "Payment request not found or expire.";
			return;
		}

		From = await context.PaymentRequestFroms.FindAsync(Request.FromId);

		if (Request.PaymentStatus == "SUCCESS") 
		{
			SuccessMessage = "Payment request has been paid successfully.";
			return;
		}

		var result = await mastercardService.VerifyPayment(Id.ToString());

		if (result == "SUCCESS")
		{
			Request.PaidOn = DateTime.Now;
			Request.PaymentStatus = result;
			context.Entry(Request).State = EntityState.Modified;
			await context.SaveChangesAsync();

			SuccessMessage = "Payment request has been paid successfully.";
		}
		else
		{
			ErrorMessage = "An error has been occour during payment processing.";
		}
	}




	private async Task Print()
	{
		await js.InvokeVoidAsync("window.print");
	}
}