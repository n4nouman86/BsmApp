using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.PaymentRequestPages;

public partial class EditPage
{
	[Parameter]
	public Guid Id { get; set; }
	private IEnumerable<PaymentRequestFrom>? Froms { get; set; }
	private PaymentRequest Input { get; set; } = new PaymentRequest();




	protected override async Task OnInitializedAsync()
	{
		using var context = contextFactory.CreateDbContext();
		Froms = await context.PaymentRequestFroms.ToListAsync();
		Input = await context.PaymentRequests.FindAsync(Id) ?? new PaymentRequest();
	}




	private async Task UpdatePaymentRequest()
	{
		using var context = contextFactory.CreateDbContext();
		context.Entry(Input).State = EntityState.Modified;
		await context.SaveChangesAsync();

		notificationService
		.Notify(Radzen.NotificationSeverity.Success, "Success", "Payment Request updated successfully");
	}
}