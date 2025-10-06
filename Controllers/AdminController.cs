using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserManagementAPI.Data;
using UserManagementAPI.DTOs;
using UserManagementAPI.Models;
using System.IO;
using UserManagementAPI.Services;

namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary _cloudinary;
        private readonly AuditService _auditService;


        public AdminController(ApplicationDbContext context, Cloudinary cloudinary, AuditService auditService)
        {
            _context = context;
            _cloudinary = cloudinary;
            _auditService = auditService;
        }

        // GET: api/Admin/users?page=1&pageSize=20&q=search
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? q = null)
        {
            // GET: api/Admin/users?page=1&pageSize=20&q=search
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 20;

                var query = _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Status == "Activo")   // 🔹 Solo usuarios activos
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    q = q.Trim().ToLower();
                    query = query.Where(u => u.Username.ToLower().Contains(q) || u.Email.ToLower().Contains(q));
                }

                var total = await query.CountAsync();
                var users = await query
                    .OrderBy(u => u.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new AdminUserDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                        RoleId = u.RoleId,
                        RoleName = u.Role.Rol,
                        Status = u.Status,
                        ProfilePictureUrl = u.ProfilePictureUrl,
                        CreatedAt = u.CreatedAt,
                        UpdatedAt = u.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(new { data = users, total, page, pageSize });

        }

        // GET: api/Admin/users/5
        [HttpGet("users/{id:int}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            var dto = new AdminUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.Role?.Rol ?? "",
                Status = user.Status,
                ProfilePictureUrl = user.ProfilePictureUrl,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt

            };

            return Ok(dto);
        }

        // POST: api/Admin/users
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromForm] CreateUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // validar email único
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("El correo ya está en uso.");

            // validar role existe
            var role = await _context.Roles.FindAsync(dto.RoleId);
            if (role == null) return BadRequest("Rol no válido.");

            // crear user
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = dto.RoleId,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow
            };

            // subir foto si viene
            if (dto.ProfilePicture != null && dto.ProfilePicture.Length > 0)
            {
                var uploadedUrl = await UploadProfilePictureAsync(dto.ProfilePicture);
                if (uploadedUrl == null) return BadRequest("Error al subir la imagen.");
                user.ProfilePictureUrl = uploadedUrl;
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // audit
            await AddAuditAsync(userId: GetCurrentAdminId(), action: "CreateUser", description: $"Admin creó usuario {user.Email} (Id:{user.Id})");

            var resultDto = new AdminUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = role.Rol,
                Status = user.Status,
                ProfilePictureUrl = user.ProfilePictureUrl,
                CreatedAt = user.CreatedAt
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, resultDto);
        }

        // PUT: api/Admin/users/5
        [HttpPut("users/{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, [FromForm] UpdateUserByAdminDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // evitar que admin cambie su propio rol a algo que lo deje fuera? (opcional)
            var currentAdminId = GetCurrentAdminId();

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                if (await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id))
                    return BadRequest("El correo ya está en uso por otro usuario.");
                user.Email = dto.Email;
            }

            if (!string.IsNullOrWhiteSpace(dto.Username)) user.Username = dto.Username;
            if (!string.IsNullOrWhiteSpace(dto.Status)) user.Status = dto.Status;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            if (dto.RoleId.HasValue && dto.RoleId.Value != user.RoleId)
            {
                var newRole = await _context.Roles.FindAsync(dto.RoleId.Value);
                if (newRole == null) return BadRequest("Rol no válido.");
                user.RoleId = dto.RoleId.Value;
            }

            if (dto.ProfilePicture != null && dto.ProfilePicture.Length > 0)
            {
                var uploadedUrl = await UploadProfilePictureAsync(dto.ProfilePicture);
                if (uploadedUrl == null) return BadRequest("Error al subir la imagen.");
                user.ProfilePictureUrl = uploadedUrl;
            }

            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            await AddAuditAsync(userId: currentAdminId, action: "UpdateUser", description: $"Admin actualizó usuario Id:{user.Id}");

            return Ok("Usuario actualizado correctamente.");
        }

        // DELETE: api/Admin/users/5
        [HttpDelete("users/{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
                var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
                if (user == null) return NotFound();

                var currentAdminId = GetCurrentAdminId();

                if (user.Id == currentAdminId)
                    return BadRequest("No puedes eliminar tu propia cuenta.");

                if (user.Role != null && user.Role.Rol == "Admin")
                {
                    var adminCount = await _context.Users
                        .CountAsync(u => u.RoleId == user.RoleId && u.Status == "Activo");
                    if (adminCount <= 1)
                        return BadRequest("No se puede desactivar el último administrador.");
                }

                user.Status = "Inactivo"; // Soft delete
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                await AddAuditAsync(
                    userId: currentAdminId,
                    action: "SoftDeleteUser",
                    description: $"Admin desactivó usuario Id:{user.Id}, Email:{user.Email}"
                );

                return Ok("Usuario desactivado correctamente.");

        }

        [ApiController]
        [Route("api/admin")]
        [Authorize(Roles = "Admin")]
        public class AdminLogsController : ControllerBase
        {
            private readonly ApplicationDbContext _context;

            public AdminLogsController(ApplicationDbContext context)
            {
                _context = context;
            }

            [HttpGet("logs")]
            public async Task<IActionResult> GetLogs()
            {
                var logs = await _context.AuditLogs
                    .OrderByDescending(l => l.Timestamp)
                    .Take(100)
                    .ToListAsync();

                return Ok(logs);
            }
        }


        // --------- Helpers ---------
        private int GetCurrentAdminId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out int id)) return id;
            return 0;
        }
        
        private async Task<string?> UploadProfilePictureAsync(IFormFile file)
        {
            // validaciones
            var allowed = new[] { ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowed.Contains(ext)) throw new Exception("Formato de imagen no admitido.");

            if (file.Length > 5 * 1024 * 1024) // 5 MB límite admin (ajustable)
                throw new Exception("La imagen supera el tamaño máximo permitido.");

            // opcional: redimensionar con ImageSharp si quieres (no incluido aquí)
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = "profile-pictures",
                Transformation = new Transformation().Width(600).Height(600).Crop("limit")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            if (result.Error != null) return null;
            return result.SecureUrl.ToString();
        }

        private async Task AddAuditAsync(int? userId, string action, string description)
        {
            // Si no tienes AuditLogs, comenta o implementa la entidad AuditLog y DbSet
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
            catch
            {
                // no bloquear operación por fallo de auditoría
            }
        }

    }
}
