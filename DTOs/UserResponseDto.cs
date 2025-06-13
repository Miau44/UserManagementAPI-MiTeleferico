using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.DTOs
{
    public class UserResponseDto
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio GAAA")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "El email de usuario es obligatorio GAAA")]
        public required string Email { get; set; } 
        public string? ProfileImageUrl { get; set; }
        public string? Status { get; set; }
    }
}
