using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UserManagementAPI.Data;
using UserManagementAPI.Models;
using System.Security.Claims;

namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Solo usuarios logueados
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("Save")]
        public async Task<IActionResult> SaveTest([FromBody] Dictionary<string, string> respuestas)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Unauthorized();

                foreach (var r in respuestas)
                {
                    _context.TestAnswers.Add(new TestAnswer
                    {
                        UserId = int.Parse(userId),
                        QuestionKey = r.Key,
                        Answer = r.Value
                    });
                }

                await _context.SaveChangesAsync();
                return Ok(new { mensaje = "Respuestas guardadas correctamente" });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    error = dbEx.Message,
                    inner = dbEx.InnerException?.Message,
                    entries = dbEx.Entries.Select(e => e.Entity) // muestra qué entidades fallaron
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message,
                    stack = ex.StackTrace,
                    inner = ex.InnerException?.Message?? "No inner exception"
                });
            }
        }

        [HttpGet("MyTests")]
        public IActionResult MyTests()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var tests = _context.UserTests
                        .Where(t => t.UserId == userId)
                        .OrderByDescending(t => t.Fecha)
                        .ToList();
            return Ok(tests);
        }
    }
}
