using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserManagementAPI.Data;
using UserManagementAPI.DTOs;
using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly Cloudinary _cloudinary;

        public ProfileController(ApplicationDbContext context, IWebHostEnvironment env, Cloudinary cloudinary)
        {
            _context = context;
            _env = env;
            _cloudinary = cloudinary;
        }

        private string? UploadProfilePicture(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                throw new Exception("Formato de archivo no permitido. Solo JPG y PNG.");

            // Tamaño máximo 2MB
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


        // 📌 Obtener perfil del usuario autenticado
        [Authorize]
        [HttpGet("perfil")]
        public async Task<IActionResult> GetProfile()
        {
            var userName = User.Identity?.Name ?? "Invitado";
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null) return NotFound("Usuario no encontrado.");

            return Ok(new
            {
                mensaje = $"Buenazooo. Bienvenido {userName}, este es tu perfil mi causa 🍑 .",
                user.Username,
                user.Email,
                user.Status,
                user.ProfilePictureUrl
            });
        }


        // 📌 Actualizar perfil (nombre, estado y foto)
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null) return NotFound("Usuario no encontrado.");

            // Actualizar nombre y estado
            if (!string.IsNullOrWhiteSpace(dto.Username))
                user.Username = dto.Username;

            if (!string.IsNullOrWhiteSpace(dto.Status))
                user.Status = dto.Status;

            // Manejo de foto en Cloudinary
            if (dto.ProfilePicture != null && dto.ProfilePicture.Length > 0)
            {
                var extension = Path.GetExtension(dto.ProfilePicture.FileName).ToLower();
                var allowed = new[] { ".jpg", ".jpeg", ".png" };

                if (!allowed.Contains(extension))
                    return BadRequest("Formato de imagen no válido. Solo JPG y PNG.");

                if (dto.ProfilePicture.Length > 2 * 1024 * 1024)
                    return BadRequest("La imagen no puede superar los 2 MB.");

                // Subir a Cloudinary
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(dto.ProfilePicture.FileName, dto.ProfilePicture.OpenReadStream()),
                    Folder = "profile-pictures",
                    Transformation = new Transformation().Width(500).Height(500).Crop("fill") // opcional: redimensiona la foto
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                    return BadRequest(uploadResult.Error.Message);

                // Guardar URL en base de datos
                user.ProfilePictureUrl = uploadResult.SecureUrl.ToString();
            }

            await _context.SaveChangesAsync();
            return Ok("Perfil actualizado correctamente.");
        }

    }
}
