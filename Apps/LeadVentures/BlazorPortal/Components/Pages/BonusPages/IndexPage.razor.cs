using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.Text.Json;

namespace BlazorPortal.Components.Pages.BonusPages;

public partial class IndexPage
{
	private InputModel Input { get; set; } = new();
	private IEnumerable<vwBonus>? Bonuses { get; set; } = Enumerable.Empty<vwBonus>();




	protected async override Task OnParametersSetAsync()
	{
		var json = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "Input");

		if (string.IsNullOrWhiteSpace(json))
		{
			Input = new() 
			{ 
				DateFrom = DateTime.Today.AddDays(-Input.Days), 
				DateTo = DateTime.Today,
				PaymentMonth = DateTime.Today.AddMonths(-1)
			};
		}
		else
		{
			Input = JsonSerializer.Deserialize<InputModel>(json) ?? new();
		}
	}




	private async Task SaveLocalStorage()
	{
		Input.DateFrom = Input.DateTo.AddDays(-Input.Days);
		var json = JsonSerializer.Serialize(Input);
		await jsRuntime.InvokeVoidAsync("localStorage.setItem", "Input", json);
	}




	private async Task LoadBonus()
	{
		DateTime paymentFrom = new DateTime(Input.PaymentMonth.Year, Input.PaymentMonth.Month, 1);
		DateTime paymentTo = paymentFrom.AddMonths(1);

		Bonuses = Enumerable.Empty<vwBonus>();

		using var context = await DbFactory.CreateDbContextAsync();
		
		Bonuses = await context.vwBonuses
			.Where(x => 
				x.EmployeeName != null && 
				x.AgentOnboardOn >= Input.DateFrom && 
				x.AgentOnboardOn <= Input.DateTo &&
				x.InvoiceServiceFrom >= paymentFrom &&
				x.InvoiceServiceFrom < paymentTo)
			.ToListAsync();
	}



	private class InputModel
	{
		public DateTime DateFrom { get; set; }
		public DateTime DateTo { get; set; }
		public int Days { get; set; } = 90;
		public DateTime PaymentMonth { get; set; }
	}
}