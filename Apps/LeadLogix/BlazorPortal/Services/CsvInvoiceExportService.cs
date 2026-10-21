using BlazorPortal.Entities;
using BlazorPortal.MethodExtensions;
using Microsoft.JSInterop;
using System.Text;

namespace BlazorPortal.Services;

public class CsvInvoiceExportService
{
    public void CreateCsv(IJSRuntime iJSRuntime, IEnumerable<vwInvoice> invoices) 
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("Invoice No,");
        sb.Append("Due Date,");
        sb.Append("Full Name,");
        sb.Append("Agent Email,");
        sb.Append("Total Amount,");

        sb.AppendLine();

        foreach (var item in invoices)
        {
            sb.Append(GetFormattedString(item.InvoiceNo.ToString()) + ",");
            sb.Append(GetFormattedString(item.DueDate.ToString("dd/MM/yyyy")) + ",");
            sb.Append(GetFormattedString(item.FullName) + ",");
            sb.Append(GetFormattedString(item.AgentEmail) + ",");
            sb.Append(GetFormattedString(item.TotalCost.ToString()) + ",");

            sb.AppendLine();
        }

        byte[] fileContents;
        fileContents = UTF8Encoding.Default.GetBytes(sb.ToString());

        iJSRuntime.InvokeAsync<CsvLeadExportService>(
            "saveAsFile",
            "exportinvoices.csv",
            Convert.ToBase64String(fileContents)
        );
    }







    private string GetFormattedString(string val)
    {
        string str = $"\"{val}\"";
        
        return str;
    }
}
