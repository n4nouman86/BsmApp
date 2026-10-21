using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorPortal.Entities;

public partial class vwcTotalDayTransfer
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
    public double? ChartTotalVal
    {
        get
        {
            return Total;
        }
    }


	[NotMapped]
	public double? ChartBillableVal
	{
		get
		{
			return BillAble;
		}
	}

	[NotMapped]
	public double? ChartNonBillableVal
	{
		get
		{
			return NonBillAble;
		}
	}
}
