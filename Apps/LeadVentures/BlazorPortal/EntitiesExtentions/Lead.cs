using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorPortal.Entities
{
    public partial class Lead
    {
        [NotMapped]
        public TimeSpan LastTransferAgo 
        {
            get
            {
                return DateTime.Now - TransferOn;
            } 
        }
    }
}
