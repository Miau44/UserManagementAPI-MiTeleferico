using System.Collections.Generic;

namespace UserManagementAPI.Models
{
    public class Role
    {
        public int Id { get; set; }           // Primary Key
        public required string Rol { get; set; }      // Nombre del rol (ej: "Admin", "User")

        // Relación uno a muchos con Users
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
