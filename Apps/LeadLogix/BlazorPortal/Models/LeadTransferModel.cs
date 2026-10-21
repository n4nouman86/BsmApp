namespace BlazorPortal.Models
{
    public class LeadTransferModel
    {
        public Guid AgentId { get; set; }
        public Guid LeadId { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
