using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace BlazorPortal.Components.Pages.AgentPages;

public partial class IndexPage
{
    private IEnumerable<vwAgent>? Agents { get; set; }
    private int count = 0;
    private bool isLoading = false;




    protected override async Task OnInitializedAsync()
    {
        await LoadData(new LoadDataArgs { Skip = 0, Top = 10 });
    }




    private InputSearchModel Input { get; set; } = new InputSearchModel();
    private async Task SearchAgent()
    {
        await LoadData(new LoadDataArgs { Skip = 0, Top = 10 });
    }




    private async Task LoadData(LoadDataArgs args)
    {
        isLoading = true;

        await using var context = await DbFactory.CreateDbContextAsync();

        var query = context.vwAgents.AsQueryable();

        if (!string.IsNullOrWhiteSpace(Input.SearchTerm))
        {
            query = query.Where(x => x.FullName.Contains(Input.SearchTerm));
        }

        Agents = await query
			.OrderByDescending(x => x.CreateOn)
			.Skip(args.Skip ?? 0)
            .Take(args.Top ?? 10)
            .ToListAsync();

        count = query.Count();

        isLoading = false;
    }




    private sealed class InputSearchModel
    {
        public string? SearchTerm { get; set; }
    }
}