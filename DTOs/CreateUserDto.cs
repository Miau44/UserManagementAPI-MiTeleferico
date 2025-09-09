using Microsoft.AspNetCore.Http;
namespace UserManagementAPI.DTOs
{
    public class CreateUserDto
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int RoleId { get; set; }           // rol a asignar (Admin o User)
        public string? Status { get; set; }
        public IFormFile? ProfilePicture { get; set; } // opcional
    }
}
