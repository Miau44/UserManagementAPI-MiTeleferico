using UserManagementAPI.Data;
using UserManagementAPI.Models;

namespace UserManagementAPI.Services
{
    public class AuditService
    {
        private readonly ApplicationDbContext _context;
        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(int? userId, string action, string description)
        {
            try
            {
                if (_context.Database.CanConnect())
                {
                    _context.AuditLogs.Add(new AuditLog
                    {
                        UserId = userId,
                        Action = action,
                        Description = description,
                        Timestamp = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                }
            }
            catch { /* no bloquear */ }
        }
    }
}
