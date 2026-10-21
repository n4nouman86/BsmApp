using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace BlazorPortal.Components.Pages.AppUserPages;

public partial class AppUserIndexPage
{
    private IEnumerable<vwAppUser>? AppUsers { get; set; }
    private int count = 0;
    private bool isLoading = false;




    protected override async Task OnInitializedAsync()
    {
        await LoadData(new LoadDataArgs { Skip = 0, Top = 10});
    }




    private InputSearchModel Input { get; set; } = new InputSearchModel();
    private async Task SearchAppUser()
    {
        await LoadData(new LoadDataArgs { Skip = 0, Top = 10 });
    }




    private async Task LoadData(LoadDataArgs args)
    {
        isLoading = true;

        await using var db = await DbFactory.CreateDbContextAsync();

        var query = db.vwAppUsers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(Input.SearchTerm))
        {
            query = query.Where(x => x.FullName.Contains(Input.SearchTerm));
        }

        AppUsers = await query
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