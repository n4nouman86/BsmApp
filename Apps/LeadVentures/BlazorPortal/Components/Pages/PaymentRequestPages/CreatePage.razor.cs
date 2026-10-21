using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.PaymentRequestPages;

public partial class CreatePage
{
	private IEnumerable<PaymentRequestFrom>? Froms{ get; set; }
	private PaymentRequest Input { get; set; } = new PaymentRequest();
	private bool disableCreateButton = false;




	protected override async Task OnInitializedAsync()
	{
		using var context = contextFactory.CreateDbContext();
		Froms = await context.PaymentRequestFroms.ToListAsync();
		Input.DueDate = DateTime.Now;
	}




	private async Task CreateExInvoice()
	{
		using var context = contextFactory.CreateDbContext();
		disableCreateButton = true;

		Input.Id = Guid.CreateVersion7();
		Input.CreatedOn = DateTime.Now;

		await context.AddAsync(Input);
		await context.SaveChangesAsync();

		string paymentLink = navigationManager.ToAbsoluteUri(string.Concat("PaymentRequest/", Input.Id.ToString())).ToString();
		var from = Froms!.First(x => x.Id == Input.FromId);

		await emailService.SendPaymentRequestLinkAsync(Input, from, paymentLink);

		Input = new PaymentRequest() { DueDate = DateTime.Now };

		notificationService
		.Notify(Radzen.NotificationSeverity.Success, "Success", "Payment request created successfully.");

		disableCreateButton = false;
	}
}