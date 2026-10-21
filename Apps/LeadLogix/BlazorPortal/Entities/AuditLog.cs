namespace BlazorPortal.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string EntityState { get; set; } = string.Empty;
        public string EntityMetadata { get; set; } = string.Empty;
        public DateTime EntityStateOn { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Memo { get; set; } = string.Empty;
    }
}
