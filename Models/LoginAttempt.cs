namespace UserManagementAPI.Models
{
    public class LoginAttempt
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public DateTime AttemptTime { get; set; } = DateTime.Now;
        public bool IsSuccessful { get; set; }
        public required string IPAddress { get; set; }
    }
}
