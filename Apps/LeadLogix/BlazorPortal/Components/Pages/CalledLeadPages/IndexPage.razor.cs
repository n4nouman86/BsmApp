using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace BlazorPortal.Components.Pages.CalledLeadPages;

public partial class IndexPage
{
	private IEnumerable<vwCalledLead>? CalledLeads { get; set; }
	private InputPhoneSearch Input { get; set; } = new();




	protected override async Task OnInitializedAsync()
	{
		await LoadCalledLeads();
	}




	private async Task LoadCalledLeads()
	{
		await using var context = await DbFactory.CreateDbContextAsync();
	
		CalledLeads = await context.vwCalledLeads
			.OrderByDescending(x => x.CalledOn)
			.Take(25)
			.ToListAsync();
	}




	private async Task SearchByPhone()
	{
		if (string.IsNullOrWhiteSpace(Input.PhoneNumber))
		{
			return;
		}

		await using var context = await DbFactory.CreateDbContextAsync();

		var query = context.vwCalledLeads.AsQueryable();

		if (!string.IsNullOrWhiteSpace(Input.PhoneNumber))
		{
			var phone = Input.PhoneNumber.Trim();
			query = query.Where(x => x.PhoneNumber.Contains(phone));
		}

		CalledLeads = await query
			.OrderByDescending(x => x.CalledOn)
			.ToListAsync();
	}




	private async Task OpenEdit(int id)
	{
		var result = await dialogService.OpenAsync<EditPage>(
			"Edit Called Lead",
			new Dictionary<string, object?> { { "Id", id } },
			new DialogOptions { Width = "90%", CloseDialogOnOverlayClick = true });

		if (result is int)
		{
			await SearchByPhone();
		}
	}




	private sealed class InputPhoneSearch
	{
		public string? PhoneNumber { get; set; }
	}
}
