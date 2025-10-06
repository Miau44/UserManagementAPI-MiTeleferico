using Azure.Core;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UserManagementAPI.Data;
using UserManagementAPI.DTOs;
using UserManagementAPI.Models;
using UserManagementAPI.Services;


namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;
        private readonly Cloudinary _cloudinary;
        private readonly AuditService _auditService;

        public AuthController(ApplicationDbContext context, IConfiguration config, EmailService emailService, Cloudinary cloudinary, AuditService auditService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
            _cloudinary = cloudinary;
            _auditService = auditService;
        
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("TAAAAAAARDE, El correo ya está registrado :b");


            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Rol == "User");
            if (defaultRole == null)
            {
                return StatusCode(500, "Rol por defecto 'User' no existe en la base de datos.");
            }

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow,
                RoleId = defaultRole.Id
            };
            

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Usuario registrado con éxito mi rey.");

        }
        

        private string? UploadProfilePicture(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                throw new Exception("Formato no permitido. Solo JPG y PNG.");

            if (file.Length > 2 * 1024 * 1024)
                throw new Exception("La imagen no debe superar los 2 MB.");

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = "profile-pictures"
            };

            var uploadResult = _cloudinary.Upload(uploadParams);
            return uploadResult.SecureUrl.ToString();
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users
                .Include(u => u.Role) // 🔹 Cargar el rol
                .FirstOrDefaultAsync(u => u.Email == dto.Email);


            if (user == null)
                return Unauthorized("Credenciales inválidas, PICHOON.");

            // Verificar si está bloqueado
            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            {
                var minutosRestantes = (user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes;
                return Unauthorized($"Cuenta bloqueada. Intenta en {Math.Ceiling(minutosRestantes)} minutos, mi king.");
            }

            // Verificación de contraseña
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;

                // Bloquear después de 5 intentos
                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
                    user.FailedLoginAttempts = 0; // opcional: reiniciar contador tras bloqueo
                }

                await _context.SaveChangesAsync();

                await _auditService.LogAsync(null, "LoginFailed", $"Intento fallido para email: {dto.Email}");

                return Unauthorized("Credenciales inválidas, PICHOON.");
            }

            // Inicio de sesión exitoso: reiniciar contador
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            await _context.SaveChangesAsync();

            // ✅ Generar el token usando tu función
            var token = GenerateJwtToken(user);
            await _auditService.LogAsync(user.Id, "LoginSuccess", $"Usuario {user.Username} inició sesión correctamente");
            // ✅ Retornar el token como parte de la respuesta
            return Ok(new
            {
                message = "Inicio de sesión exitoso mi king 👑",
                token = token

            });
            

        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return NotFound("No papi, acabo de chequear y tu Correo no esta registrado.");

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var expiration = DateTime.UtcNow.AddMinutes(30);

            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
                Expiration = expiration,
                IsUsed = false,
                User = user
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            // Codificar token para URL
            var resetLink = $"{dto.ClientAppUrl}?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";

            await _emailService.SendEmailAsync(user.Email, "Recuperación de contraseña",
                $"Haz clic en el siguiente enlace para restablecer tu contraseña: {resetLink}");

            await _auditService.LogAsync(user.Id, "PasswordResetRequest", $"Usuario {user.Username} solicitó recuperación de contraseña");

            return Ok("Se envió un enlace de recuperación al correo.");
        }



        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            

            var reset = await _context.PasswordResetTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == dto.Token && !r.IsUsed && r.Expiration > DateTime.UtcNow);

            if (reset == null)
                return BadRequest("Token inválido o expirado.");

            var user = reset.User; // <-- aquí declaras 'user'

            reset.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            reset.IsUsed = true;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(user.Id, "PasswordReset", $"Usuario {user.Username} actualizó su contraseña");

            return Ok("Contraseña actualizada con éxito.");
        }


        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private string GenerateJwtToken(User user)
        {

            var jwtConfig = _config.GetSection("Jwt");
            var key = jwtConfig["Key"];

            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("JWT Key no configurada correctamente en appsettings.json.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var issuer = jwtConfig["Issuer"] ?? throw new InvalidOperationException("Issuer no configurado.");
            var audience = jwtConfig["Audience"] ?? throw new InvalidOperationException("Audience no configurado.");
            var expiresIn = jwtConfig["ExpiresInMinutes"] ?? throw new InvalidOperationException("ExpiresInMinutes no configurado.");

            var claims = new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("Role", user.Role.Rol) // <-- importante
                };

            var token = new JwtSecurityToken(
                issuer: jwtConfig["Issuer"],
                audience: jwtConfig["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtConfig["ExpiresInMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private async Task AddAuditAsync(int? userId, string action, string description)
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


    }

}
