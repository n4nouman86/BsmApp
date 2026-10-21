using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorPortal.Entities;

public partial class vwAppUser
{
    [NotMapped]
    public string LastSeen 
    {
        get
        {
            string output = string.Empty;

            var diff = DateTimeOffset.Now - LastOnlineOn;

            if (diff is null)
            {
                return output;
            }

            var tm = diff.Value.TotalMinutes;

            if (tm <= 1)
            {
                output = "Online";
                
                return output;
            }

            if (tm < 60)
            {
                output = $"{tm.ToString("N0")} mins ago";
                
                return output;
            }

            if (tm > 1440)
            {
                int day = Convert.ToInt32(Math.Floor(tm / 1440));
                output = $"{day} days ago";

                return output;
            }

            if (tm > 60)
            {
                int hour = Convert.ToInt32(Math.Floor(tm / 60));
                output = $"{hour} hours ago";

                return output;
            }

            return output;
        }  
    }
}
