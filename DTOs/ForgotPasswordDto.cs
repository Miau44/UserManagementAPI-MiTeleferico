using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.DTOs
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [EmailAddress]
        public required string Email { get; set; }
        public required string ClientAppUrl { get; set; } // ej: http://localhost:5173/reset-password
    }
}
