using BlazorPortal.MethodExtensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorPortal.Entities;

public partial class vwStatusAgent
{
    [NotMapped]
    public double PauseTimer
    {
        get
        {
            if (PauseTill is null)
            {
                return 0;
            }

            if (PauseTill.Value.ToServerDateTimeOffset().DateTime < DateTimeOffset.Now.ToServerDateTimeOffset().DateTime)
            {
                return 0;
            }

            var diff = PauseTill.Value.ToServerDateTimeOffset().DateTime - DateTimeOffset.Now.ToServerDateTimeOffset().DateTime;

            return diff.TotalMinutes;
        }
    }






    [NotMapped]
    public int? LastTransfer 
    {
        get
        {
            int? result = CoolOffTime - LastTransferMinsAgo;

            if (result < 0)
            {
                return 0;
            }

            return result;
        }
    }





    [NotMapped]
    public string CardTextColor 
    { 
        get 
        {
            string color = "";

            if (CardColor == "rz-background-color-danger-dark")
            {
                color = "rz-color-white";
            }

            if (CardColor == "rz-background-color-info-dark")
            {
                color = "rz-color-white";
            }

            if (CardColor == "rz-background-color-black")
            {
                color = "rz-color-white";
            }

            if (CardColor == "rz-background-color-success-darker")
            {
                color = "rz-color-white";
            }

            return color;
        } 
    }






    [NotMapped]
    public string CardColor 
    {
        get
        {
            if (DailyCap == TodayTransfer)
            {
                return "rz-background-color-success-darker";
            }

            if (!IsOnline || CampaignStatus != "Active")
            {
                return "rz-background-color-black";
            }

            var dtn = DateTimeOffset.Now.ToLacalDateTimeOffset(TimeZoneInfoId).TimeOfDay;
            var sf = ScheduleFrom?.ToLacalDateTimeOffset(TimeZoneInfoId).TimeOfDay;
            var st = ScheduleTo?.ToLacalDateTimeOffset(TimeZoneInfoId).TimeOfDay;

            if (!(dtn > sf && dtn < st))
            {
                return "";
            }

            if (TodayTransfer >= DailyCap) 
            {
                return "rz-background-color-danger-dark";
            }

            if (LastTransfer > 0)
            {
                return "rz-background-color-danger-dark";
            }

            if (PauseTimer > 0)
            {
                return "rz-background-color-danger-dark";
            }

            return "rz-background-color-info-dark";
        } 
    }





    public string LastTransferAgo 
    { 
        get 
        {
            string output = string.Empty;

            if (LastTransferMinsAgo is null)
            {
                output = "no transfer";
            }
            else if (LastTransferMinsAgo < 60)
            {
                output = $"{LastTransferMinsAgo} mints ago";
            }
            else if (LastTransferMinsAgo < 1440)
            {
                output = $"{LastTransferMinsAgo / 60} hours ago";
            }
            else if (LastTransferMinsAgo < 43200)
            {
                output = $"{LastTransferMinsAgo / 1440} days ago";
            }
            else
            {
                output = $"{LastTransferMinsAgo / 43200} months ago";
            }

            return output;
        } 
    }
}
