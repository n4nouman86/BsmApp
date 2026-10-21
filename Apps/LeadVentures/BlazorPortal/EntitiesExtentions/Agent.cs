using BlazorPortal.MethodExtensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorPortal.Entities
{
    public partial class Agent
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
    }
}
