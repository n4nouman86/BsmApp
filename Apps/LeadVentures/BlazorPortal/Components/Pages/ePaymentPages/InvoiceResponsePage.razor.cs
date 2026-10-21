using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace BlazorPortal.Components.Pages.ePaymentPages;

public partial class InvoiceResponsePage
{
	[Parameter]
	public Guid Id { get; set; }
	private string? ErrorMessage { get; set; }
	public string? SuccessMessage { get; set; }
	private Invoice? Invoice { get; set; }



	protected override async Task OnInitializedAsync()
	{
		using var context = contextFactory.CreateDbContext();
		Invoice = await context.Invoices.FirstOrDefaultAsync(x => x.Id == Id);
		
		if (Invoice is null)
		{
			ErrorMessage = "Invoice not found.";
			return;
		}

		//if (Invoice.PaymentStatus == "SUCCESS") 
		//{
		//	SuccessMessage = "Invoice has been paid successfully.";
		//	return;
		//}

		var result = await mastercardService.VerifyPayment(Id.ToString());

		if (result == "SUCCESS") 
		{
			Invoice.PaidOn = DateTime.Now;
			//Invoice.PaymentStatus = result;
			context.Entry(Invoice).State = EntityState.Modified;
			await context.SaveChangesAsync();

			SuccessMessage = "Invoice has been paid successfully.";
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