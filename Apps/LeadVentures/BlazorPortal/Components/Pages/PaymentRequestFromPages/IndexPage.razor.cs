using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.PaymentRequestFromPages;

public partial class IndexPage
{
	private IEnumerable<PaymentRequestFrom>? Froms { get; set; }




	protected override async Task OnInitializedAsync()
	{
		using var context = contextFactory.CreateDbContext();
		Froms = await context.PaymentRequestFroms
			.OrderByDescending(x => x.Id)
			.ToListAsync();
	}
}