using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace BlazorPortal.Components.Pages.LeadPages;

public partial class LeadDispositionPage
{
    private IEnumerable<vwLead>? Leads { get; set; }
    private int count = 0;
    private bool isLoading = false;




    protected override async Task OnInitializedAsync()
    {
        await LoadLeads(new LoadDataArgs { Skip = 0, Top = 10 });
    }








    private async Task LoadLeads(LoadDataArgs args)
    {
        isLoading = true;

        if (args is not null && args.Skip is not null && args.Top is not null)
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            var query = db.vwLeads.AsQueryable();

            Leads = await query.Where(x => x.DispositionTypeId != null).OrderByDescending(x => x.TransferOn).Skip(args.Skip.Value).Take(args.Top.Value).ToListAsync();

            count = await query.CountAsync();
        }

        isLoading = false;
    }
}