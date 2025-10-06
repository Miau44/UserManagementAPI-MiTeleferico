namespace UserManagementAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }

        // URL pública de la foto (Cloudinary)
        public string? ProfilePictureUrl { get; set; }


        // Estado legible (opcional): "Activo", "Suspendido", "Eliminado", etc.
        public string? Status { get; set; }


        // Soft delete - control interno para saber si el usuario está "activo"
        public bool IsActive { get; set; } = true;

        // Auditoría
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Protección contra fuerza bruta
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }

        // Relación con Role
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        // Navegación
        public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();

        // ===== NUEVOS CAMPOS PARA ASIGNACIÓN DE ESTACIÓN =====
        public string? ZonaSeleccionada { get; set; }      // Zona o barrio
        public string? Calle { get; set; }               // Calle del usuario
        public double? Latitud { get; set; }             // Coordenadas
        public double? Longitud { get; set; }
        public string? EstacionAsignada { get; set; }    // Estación más cercana
        public string? LineaAsignada { get; set; }       // Línea correspondiente
    }
}
