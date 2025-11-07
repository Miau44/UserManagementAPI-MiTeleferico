using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagementAPI.Models
{
    public class UserTest
    {
        [Key]
        public int Id { get; set; }

        // Relación con el usuario
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        // Guardaremos las respuestas en JSON
        [Required]
        public string? RespuestasJson { get; set; }

        public DateTime Fecha { get; set; }
    }
}
