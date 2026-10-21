using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace BlazorPortal.Components.Pages.PaymentRequestPages;

public partial class IndexPage
{
	private IEnumerable<vwPaymentRequest>? Requests { get; set; }




	protected override async Task OnInitializedAsync()
	{
		using var context = contextFactory.CreateDbContext();
		Requests = await context.vwPaymentRequests
			.OrderByDescending(x => x.CreatedOn)
			.Take(100)
			.ToListAsync();
	}




	private async Task UnpaidRequests()
	{
		using var context = contextFactory.CreateDbContext();
		Requests = await context.vwPaymentRequests
			.Where(x => x.PaidOn == null)
			.OrderByDescending(x => x.CreatedOn)
			.ToListAsync();
	}




	private async Task DeleteRequest(Guid id)
	{
		using var context = contextFactory.CreateDbContext();
		var request = await context.PaymentRequests.FindAsync(id);

		if (request is null)
		{
			return;
		}

		if (request.PaidOn.HasValue)
		{
			return;
		}

		var result = await dialogService
		.Confirm(
			$"{request.ClientName} invoice of {request.Amount:C0}?", 
			"Delete Payment Request", 
			new Radzen.ConfirmOptions() { OkButtonText = "Yes", CancelButtonText = "No" });

		if (result is null)
		{
			return;
		}

		if (result == false)
		{
			return;
		}

		context.Entry(request).State = EntityState.Deleted;
		await context.SaveChangesAsync();

		Requests = Requests?.Where(x => x.Id != id).ToList();

		notificationService
		.Notify(Radzen.NotificationSeverity.Success, "Delete", "Payment request is deleted successfully.");
	}




	private async Task CopyLink(Guid id)
	{
		string paymentLink = navigationManager.ToAbsoluteUri(string.Concat("PaymentRequest/", id.ToString())).ToString();
		await js.InvokeVoidAsync("navigator.clipboard.writeText", paymentLink);

		notificationService
		.Notify(Radzen.NotificationSeverity.Success, "Payment Link", "Payment link copied to clipboard.");
	}
}