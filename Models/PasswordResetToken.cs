namespace UserManagementAPI.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public required string Token { get; set; }
        public DateTime Expiration { get; set; }
        public bool IsUsed { get; set; } = false;

        // Relación con User
        public required User User { get; set; }
    }
}
