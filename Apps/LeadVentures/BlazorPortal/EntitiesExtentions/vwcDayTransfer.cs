using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorPortal.Entities;

public partial class vwcDayTransfer
{
    [NotMapped]
    public string? ChartLabel
    {
        get
        {
            return TransferDate?.ToString("dd MMM");
        }
    }




    [NotMapped]
    public double? ChartVal { 
        get
        {
            return Total;
        } 
    }
}
