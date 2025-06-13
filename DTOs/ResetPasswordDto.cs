using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.DTOs
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "El token es obligatorio :b")]
        public required string Token { get; set; } 

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "La nueva contraseña debe contener al menos una mayúscula, una minúscula y un número.")]
        public required string NewPassword { get; set; }
    }
}
