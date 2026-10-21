using BlazorPortal.Entities;
using BlazorPortal.MethodExtensions;
using Microsoft.JSInterop;
using System.Text;

namespace BlazorPortal.Services;

public class CsvLeadExportService
{
    public void CreateCsv(IJSRuntime iJSRuntime, IEnumerable<vwLead> leads, string zoneInfoId) 
    {
        var fl = leads.FirstOrDefault();
        if (fl is null) fl = new vwLead();

        StringBuilder sb = new StringBuilder();

        sb.Append("Phone Number,");
        sb.Append("Full Name,");
        sb.Append("Date Of Birth,");
        sb.Append("Address,");
        sb.Append("State,");
        sb.Append("Current Auto Carrier,");
        sb.Append("Any Lapses,");
        sb.Append("Make And Model,");
        sb.Append("Accidents,");
        sb.Append("Tickets,");
        sb.Append("Home Owner,");
        sb.Append("Home Type,");
        sb.Append("Have Home Claims,");
        sb.Append("Current Home Carrier,");
        sb.Append("Any Claim In Past 3 Years,");
        sb.Append("Transfer On,");
        sb.Append("Agent Memo,");
        sb.Append("Disposition On,");
        sb.Append("Disposition Type Name,");
        sb.Append("Disposition Comment,");
        sb.Append("Verifier Comment,");
        sb.Append("Disposition Status Name,");
        sb.Append(string.IsNullOrWhiteSpace(fl.CustomLabel1) ? "CustomLabel1," : GetFormattedString(fl.CustomLabel1) + ",");
        sb.Append(string.IsNullOrWhiteSpace(fl.CustomLabel2) ? "CustomLabel2," : GetFormattedString(fl.CustomLabel2) + ",");
        sb.Append(string.IsNullOrWhiteSpace(fl.CustomLabel3) ? "CustomLabel3," : GetFormattedString(fl.CustomLabel3) + ",");
        sb.Append(string.IsNullOrWhiteSpace(fl.CustomLabel4) ? "CustomLabel4," : GetFormattedString(fl.CustomLabel4) + ",");
        sb.Append(string.IsNullOrWhiteSpace(fl.CustomLabel5) ? "CustomLabel5," : GetFormattedString(fl.CustomLabel5) + ",");
        sb.Append(string.IsNullOrWhiteSpace(fl.CustomLabel6) ? "CustomLabel6," : GetFormattedString(fl.CustomLabel6) + ",");
        sb.Append(string.IsNullOrWhiteSpace(fl.CustomLabel7) ? "CustomLabel7," : GetFormattedString(fl.CustomLabel7) + ",");
        sb.Append(string.IsNullOrWhiteSpace(fl.CustomLabel8) ? "CustomLabel8," : GetFormattedString(fl.CustomLabel8) + ",");
        sb.Append(string.IsNullOrWhiteSpace(fl.CustomLabel9) ? "CustomLabel9," : GetFormattedString(fl.CustomLabel9) + ",");
        sb.Append(string.IsNullOrWhiteSpace(fl.CustomLabel10) ? "CustomLabel10," : GetFormattedString(fl.CustomLabel10) + ",");
        sb.Append("Is Billable");

        sb.AppendLine();

        foreach (var item in leads)
        {
            sb.Append(GetFormattedString(item.PhoneNumber) + ",");
            sb.Append(GetFormattedString(item.FullName) + ",");
            sb.Append(GetFormattedString(item.DateOfBirth) + ",");
            sb.Append(GetFormattedString(item.Address) + ",");
            sb.Append(GetFormattedString(item.State) + ",");
            sb.Append(GetFormattedString(item.CurrentAutoCarrier) + ",");
            sb.Append(GetFormattedString(item.AnyLapses) + ",");
            sb.Append(GetFormattedString(item.MakeAndModel) + ",");
            sb.Append(GetFormattedString(item.Accidents) + ",");
            sb.Append(GetFormattedString(item.Tickets) + ",");
            sb.Append(GetFormattedString(item.HomeOwner) + ",");
            sb.Append(GetFormattedString(item.HomeType) + ",");
            sb.Append(GetFormattedString(item.HaveHomeClaims) + ",");
            sb.Append(GetFormattedString(item.CurrentHomeCarrier) + ",");
            sb.Append(GetFormattedString(item.AnyClaimInPastThreeYears) + ",");
            sb.Append(item.TransferOn.ToLacalDateTimeOffset(zoneInfoId).DateTime + ",");
            sb.Append(GetFormattedString(item.CallMemo) + ",");
            sb.Append(item.DispositionOn?.ToLacalDateTimeOffset(zoneInfoId).DateTime + ",");
            sb.Append(GetFormattedString(item.DispositionTypeName) + ",");
            sb.Append(GetFormattedString(item.DispositionComment) + ",");
            sb.Append(GetFormattedString(item.VerifierComment) + ",");
            sb.Append(GetFormattedString(item.DispositionStatusName) + ",");

            sb.Append(string.IsNullOrWhiteSpace(item.Custom1) ? "n/a," : GetFormattedString(item.Custom1) + ",");
            sb.Append(string.IsNullOrWhiteSpace(item.Custom2) ? "n/a," : GetFormattedString(item.Custom2) + ",");
            sb.Append(string.IsNullOrWhiteSpace(item.Custom3) ? "n/a," : GetFormattedString(item.Custom3) + ",");
            sb.Append(string.IsNullOrWhiteSpace(item.Custom4) ? "n/a," : GetFormattedString(item.Custom4) + ",");
            sb.Append(string.IsNullOrWhiteSpace(item.Custom5) ? "n/a," : GetFormattedString(item.Custom5) + ",");
            sb.Append(string.IsNullOrWhiteSpace(item.Custom6) ? "n/a," : GetFormattedString(item.Custom6) + ",");
            sb.Append(string.IsNullOrWhiteSpace(item.Custom7) ? "n/a," : GetFormattedString(item.Custom7) + ",");
            sb.Append(string.IsNullOrWhiteSpace(item.Custom8) ? "n/a," : GetFormattedString(item.Custom8) + ",");
            sb.Append(string.IsNullOrWhiteSpace(item.Custom9) ? "n/a," : GetFormattedString(item.Custom9) + ",");
            sb.Append(string.IsNullOrWhiteSpace(item.Custom10) ? "n/a," : GetFormattedString(item.Custom10) + ",");

            sb.Append(item.IsBillable);

            sb.AppendLine();
        }





        byte[] fileContents;
        fileContents = UTF8Encoding.Default.GetBytes(sb.ToString());

        iJSRuntime.InvokeAsync<CsvLeadExportService>(
            "saveAsFile",
            "exportleads.csv",
            Convert.ToBase64String(fileContents)
        );
    }







    private string GetFormattedString(string val)
    {
        string str = $"\"{val}\"";
        
        return str;
    }
}
