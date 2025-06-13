using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
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
        public AuthController(ApplicationDbContext context, IConfiguration config, EmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("TAAAAAAARDE, El correo ya está registrado :b");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Usuario registrado con éxito mi rey.");

        }
        

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

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
                return Unauthorized("Credenciales inválidas, PICHOON.");
            }

            // Inicio de sesión exitoso: reiniciar contador
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            await _context.SaveChangesAsync();

            // ✅ Generar el token usando tu función
            var token = GenerateJwtToken(user);

            // ✅ Retornar el token como parte de la respuesta
            return Ok(new
            {
                message = "Inicio de sesión exitoso mi king 👑",
                token = token
            });
        }
        [Authorize]
        [HttpGet("perfil")]
        public IActionResult GetPerfil()
        {

            var userName = User.Identity?.Name ?? "Invitado";
            return Ok(new { mensaje = $"Buenazooo. Bienvenido {userName}, este es tu perfil mi causa 🍑 ." });
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

            var resetLink = $"{dto.ClientAppUrl}?token={token}&email={user.Email}";
            await _emailService.SendEmailAsync(user.Email, "Recuperación de contraseña",
                $"Haz clic en el siguiente enlace para restablecer tu contraseña: {resetLink}");

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

            reset.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            reset.IsUsed = true;

            await _context.SaveChangesAsync();
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
        new Claim(ClaimTypes.Email, user.Email)
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

    }
}
