using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorPortal.Entities
{
    public partial class Invoice
    {
        [NotMapped]
        public decimal CalculateTotalCost 
        { 
            get
            {
                return (BillableLeads * CostPerLead) - (DiscountAmount ?? 0);
            }

            set 
            {
                CalculateTotalCost = value;
            }
        }
    }
}
