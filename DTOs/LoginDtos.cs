using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El email es obligatorio mi king")]
        [EmailAddress]
        public required string Email { get; set; } 

        [Required(ErrorMessage = "La contraseña TAMBIEN PEE, Es obligatoria la contra  jajajaja")]
        public required string Password { get; set; } = string.Empty ;
    }
}
