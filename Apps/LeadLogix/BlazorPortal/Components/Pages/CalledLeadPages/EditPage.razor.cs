using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace BlazorPortal.Components.Pages.CalledLeadPages;

public partial class EditPage
{
	[Parameter]
	public int Id { get; set; }
	private CalledLead? Input { get; set; }
	public IEnumerable<vwState>? States { get; set; }


	protected override async Task OnInitializedAsync()
	{
		await using var context = await DbFactory.CreateDbContextAsync();
		States = await context.vwStates.ToListAsync();
		Input = await context.CalledLeads.FindAsync(Id);
	}

	private async Task Save()
	{
		if (Input is null) return;

		await using var context = await DbFactory.CreateDbContextAsync();

		context.Entry(Input).State = EntityState.Modified;
		await context.SaveChangesAsync();

		DialogService.Close(Input.Id);
	}

	private void Cancel() => DialogService.Close(false);
}
