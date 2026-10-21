#nullable disable
using BlazorPortal.Entities;
using Humanizer;
using Radzen.Blazor.Rendering;
using System.Security.Cryptography.Xml;

namespace BlazorPortal.Models;

public class PerformanceRowVM
{
    public DateTime TransferTime { get; set; }

    public int? KgBestDay { get; set; }
    public int? KgTransfersTillTime { get; set; }
    public int? KgLast30MinReport { get; set; }
    public int? KgHourlyToday { get; set; }
    public int? KgHourlyBestDay { get; set; }

    public int? BsmBestDay { get; set; }
    public int? BsmTransfersTillTime { get; set; }
    public int? BsmLast30MinReport { get; set; }
    public int? BsmHourlyToday { get; set; }
    public int? BsmHourlyBestDay { get; set; }

    public int? Score { get; set; }
    public int? HourlyScore { get; set; }


    public static List<PerformanceRowVM> BuildRows(List<PerformanceLine> lines)
    {
        lines = lines.OrderBy(x => x.TransferTime).ToList();
		
        var rows = new List<PerformanceRowVM>();

		for (int i = 0; i < lines.Count; i++)
		{	
            var preLine = i == 0 ? new() : lines[i - 1];
    		var line = lines[i];

            var row = new PerformanceRowVM();
            
            row.TransferTime = line.TransferTime;
 
            row.KgBestDay = line.KgBestDay;
            row.KgTransfersTillTime = line.KgTransfers;
            row.KgLast30MinReport = line.KgTransfers ?? line.KgTransfers - preLine.KgTransfers;

            row.BsmBestDay = line.BsmBestDay;
            row.BsmTransfersTillTime = line.BsmTransfers;
            row.BsmLast30MinReport = line.BsmTransfers ?? line.BsmTransfers - preLine.BsmTransfers;
    
			rows.Add(row);
        }

		for (int i = 0; i < rows.Count; i++)
		{
            var row = rows[i];
	
            if (i % 2 == 0)
			{
				var nextRow = rows.ElementAtOrDefault(i + 1);

				if (nextRow is not null)
				{
					row.KgHourlyToday = row.KgLast30MinReport + nextRow.KgLast30MinReport;
					row.KgHourlyBestDay = row.KgBestDay + nextRow.KgBestDay;

                    row.BsmHourlyToday = row.BsmLast30MinReport + nextRow.BsmLast30MinReport;
                    row.BsmHourlyBestDay = row.BsmBestDay + nextRow.BsmBestDay;
				}
			}
		}

		return rows;
    }
}

