using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestorMaterias.Models;
using GestorMaterias.Services;
using GestorMaterias.Data;
using System.Text.Json.Serialization;

namespace GestorMaterias.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAngularApp")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, IUsuarioService usuarioService, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _usuarioService = usuarioService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation($"Intento de inicio de sesión para el usuario: {request.Username}");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido en la solicitud de inicio de sesión");
                return BadRequest(new { success = false, message = "Datos inválidos" });
            }

            try 
            {
                var resultado = await _usuarioService.ValidarCredenciales(request.Username, request.Password);
                
                if (!resultado.success)
                {
                    _logger.LogWarning($"Inicio de sesión fallido para {request.Username}: {resultado.message}");
                    return Unauthorized(new { success = false, message = resultado.message });
                }

                var token = GenerateJwtToken(resultado.usuario);
                
                _logger.LogInformation($"Inicio de sesión exitoso para {request.Username}");
                
                var response = new { 
                    success = true, 
                    token = token,
                    user = new {
                        username = resultado.usuario.Username,
                        email = resultado.usuario.Email,
                        fullName = resultado.usuario.NombreCompleto,
                        role = resultado.usuario.EsAdministrador ? "Administrador" : "Estudiante",
                        id = resultado.usuario.Id,
                        estudianteId = resultado.usuario.EstudianteId
                    }
                };

                _logger.LogInformation($"Respuesta de autenticación: {System.Text.Json.JsonSerializer.Serialize(response)}");
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en el inicio de sesión para {request.Username}");
                return StatusCode(500, new { success = false, message = $"Error en el servidor: {ex.Message}" });
            }
        }

        [HttpGet("validate")]
        public IActionResult ValidateToken()
        {
            // Validación automática por el middleware JWT
            // Si la solicitud llega aquí, el token es válido
            return Ok(new { valid = true });
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            _logger.LogInformation($"Generando token JWT para el usuario: {usuario.Username}");
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "ClaveSecretaPorDefectoSiNoExisteEnConfig"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Username),
                new Claim(ClaimTypes.Email, usuario.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, usuario.EsAdministrador ? "Administrador" : "Estudiante"),
                new Claim("UserId", usuario.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "GestorMateriasApi",
                audience: _configuration["Jwt:Audience"] ?? "GestorMateriasAngularApp",
                claims: claims,
                expires: DateTime.Now.AddHours(12),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogInformation("Token JWT generado exitosamente");
            
            return tokenString;
        }
    }

    public class LoginRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }
}