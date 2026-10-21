using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.PaymentRequestFromPages;

public partial class EditPage
{
	[Parameter]
	public int Id { get; set; }
	private PaymentRequestFrom Input { get; set; } = new PaymentRequestFrom();




	protected override async Task OnInitializedAsync()
	{
		using var context = contextFactory.CreateDbContext();
		Input = await context.PaymentRequestFroms.FindAsync(Id) ?? new PaymentRequestFrom();
	}




	private async Task UpdatePaymentRequestFrom()
	{
		using var context = contextFactory.CreateDbContext();
		context.Entry(Input).State = EntityState.Modified;
		await context.SaveChangesAsync();

		navigationManager.NavigateTo("/PaymentRequestFromIndex");
	}
}