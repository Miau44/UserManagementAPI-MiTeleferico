using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.DTOs
{
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio GAAA")]
        [StringLength(50)]
        public required string? Username { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo no válido")]
        public required string? Email { get; set; }

        public IFormFile? ProfilePicture { get; set; } // Para archivos subidos desde un formulario multipart
    }
}
