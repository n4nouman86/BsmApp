using BlazorPortal.Entities;

namespace BlazorPortal.Components.Pages.PaymentRequestFromPages;

public partial class CreatePage
{
	private PaymentRequestFrom Input { get; set; } = new PaymentRequestFrom();



	private async Task CreateFrom()
	{
		using var context = contextFactory.CreateDbContext();
		await context.AddAsync(Input);
		await context.SaveChangesAsync();

		navigationManager.NavigateTo("/PaymentRequestFromIndex");
	}
}