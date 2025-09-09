using Microsoft.AspNetCore.Http;
namespace UserManagementAPI.DTOs
{
    public class UpdateUserByAdminDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }     // si se quiere cambiar contraseña
        public int? RoleId { get; set; }
        public string? Status { get; set; }
        public IFormFile? ProfilePicture { get; set; } // opcional
    }
}
