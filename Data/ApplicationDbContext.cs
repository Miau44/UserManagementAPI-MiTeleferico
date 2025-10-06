using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Models;

namespace UserManagementAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Estacion> Estaciones { get; set; } = null!;
        public DbSet<Zona> Zonas { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relaciones
            modelBuilder.Entity<PasswordResetToken>()
                .HasOne(p => p.User)
                .WithMany(u => u.PasswordResetTokens)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación uno a muchos
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);
        }
    }
}
