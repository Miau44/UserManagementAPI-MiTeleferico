using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.DTOs
{
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio GAAA")]
        [StringLength(50)]
        public required string Username { get; set; }

        [Required(ErrorMessage = "No se puede dejar nada en blanco rey :c")]
        public required string Status { get; set; }

        public IFormFile? ProfileImage { get; set; } // Para archivos subidos desde un formulario multipart
    }
}
