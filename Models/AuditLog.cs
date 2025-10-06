namespace UserManagementAPI.Models
{
    public class AuditLog
    {
        public int  Id { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
