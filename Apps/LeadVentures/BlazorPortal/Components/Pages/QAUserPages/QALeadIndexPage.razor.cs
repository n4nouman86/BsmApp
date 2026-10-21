using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BlazorPortal.Components.Pages.QAUserPages;

public partial class QALeadIndexPage
{
    private InputSearch Input { get; set; } = new InputSearch();

    private IEnumerable<vwLead>? Leads { get; set; }




    protected override async Task OnInitializedAsync()
    {
        await SearchLeads();
    }




    private async Task SearchLeads()
    {
        using var context = contextFactory.CreateDbContext();
        Leads = null;

        Leads = await context.vwLeads
            .Where(x => x.AgentTransferOnDate >= Input.FromDate && x.AgentTransferOnDate <= Input.ToDate)
            .OrderBy(x => x.TransferOn)
            .ToListAsync();
    }




    private sealed class InputSearch
    {
        [Required]
        public DateTime FromDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime ToDate { get; set; } = DateTime.Now;
    }
}