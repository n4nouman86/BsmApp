using System.Text;
using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace BlazorPortal.Components.Pages.LeadPages;

public partial class ExportLeadPage
{    
    private DateTimeOffset? FromDate { get; set; } = DateTime.Today;
    private DateTimeOffset? ToDate { get; set; } = DateTime.Today;
	private IEnumerable<vwState>? States { get; set; }


	private string? State { get; set; }
    private bool isExporting = false;



	protected override async Task OnInitializedAsync()
	{
		await using var db = await DbFactory.CreateDbContextAsync();
		States = await db.vwStates
        .OrderBy(x => x.Text)
        .ToListAsync();
	}



    private async Task ExportCsv()
    {
        if (FromDate is null || ToDate is null)
            return;

        isExporting = true;

        var from = new DateTimeOffset(FromDate.Value.Date, TimeSpan.Zero);
        var to = new DateTimeOffset(ToDate.Value.Date.AddDays(1), TimeSpan.Zero);

        await using var db = await DbFactory.CreateDbContextAsync();
        var query = db.vwLeads
            .Where(x => x.TransferOn >= from && x.TransferOn < to);

        if (!string.IsNullOrWhiteSpace(State))
            query = query.Where(x => x.State == State);

        var leads = await query.OrderBy(x => x.TransferOn).ToListAsync();

        var csv = BuildCsv(leads);
        var bytes = Encoding.UTF8.GetBytes(csv);
        var base64 = Convert.ToBase64String(bytes);
        var filename = $"leads-{FromDate.Value:yyyyMMdd}-{ToDate.Value:yyyyMMdd}.csv";

        await JS.InvokeVoidAsync("saveAsFile", filename, base64);

        isExporting = false;
    }




    private static string BuildCsv(IEnumerable<vwLead> leads)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Phone,FullName,Address,State");

        foreach (var lead in leads)
        {
            sb.AppendLine($"{EscapeCsvField(lead.PhoneNumber)},{EscapeCsvField(lead.FullName)},{EscapeCsvField(lead.Address)},{EscapeCsvField(lead.State)}");
        }

        return sb.ToString();
    }




    private static string EscapeCsvField(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }
}
