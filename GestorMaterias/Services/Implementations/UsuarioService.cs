using GestorMaterias.Data;
using GestorMaterias.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestorMaterias.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(ApplicationDbContext context, ILogger<UsuarioService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool success, string message, Usuario? usuario)> ValidarCredenciales(string username, string password)
        {
            _logger.LogInformation($"Validando credenciales para el usuario: {username}");

            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (usuario == null)
                {
                    _logger.LogWarning($"Usuario no encontrado: {username}");
                    return (false, "Usuario no encontrado", null);
                }

                // Comprobar si la contraseña coincide
                if (usuario.Password != password)
                {
                    _logger.LogWarning($"Contraseña incorrecta para el usuario: {username}");
                    return (false, "Credenciales inválidas", null);
                }

                _logger.LogInformation($"Credenciales validadas correctamente para el usuario: {username}");
                return (true, "Autenticación exitosa", usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al validar credenciales para el usuario: {username}");
                return (false, $"Error en el servidor: {ex.Message}", null);
            }
        }

        public async Task<(bool success, string message, Usuario? usuario)> RegistrarUsuario(Usuario usuario, string password, bool isStudentAccount = true)
        {
            // Validar que no exista el nombre de usuario
            if (await UsuarioExiste(usuario.Username))
                return (false, "El nombre de usuario ya existe", null);

            // Validar que el email no exista
            if (await EmailExiste(usuario.Email))
                return (false, "El correo electrónico ya está registrado", null);

            // En un sistema real, hashearíamos la contraseña
            usuario.Password = password; // Para implementación segura, usar un hash
            usuario.EsAdministrador = !isStudentAccount; 
            usuario.FechaRegistro = DateTime.Now;

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return (true, "Usuario registrado correctamente", usuario);
        }

        public async Task<Usuario?> ObtenerUsuarioPorId(int id)
        {
            return await _context.Usuarios
                .Include(u => u.Estudiante)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Usuario?> ObtenerUsuarioPorUsername(string username)
        {
            return await _context.Usuarios
                .Include(u => u.Estudiante)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> AsociarEstudianteAUsuario(int usuarioId, int estudianteId)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            var estudiante = await _context.Estudiantes.FindAsync(estudianteId);

            if (usuario == null || estudiante == null)
                return false;

            usuario.EstudianteId = estudianteId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UsuarioExiste(string username)
        {
            return await _context.Usuarios.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> EmailExiste(string email)
        {
            return await _context.Usuarios.AnyAsync(u => u.Email == email);
        }

        public async Task<(bool success, string message, Usuario? usuario)> RegistrarUsuarioYEstudiante(Usuario usuario, Estudiante estudiante, string password)
        {
            // Validaciones previas
            if (await UsuarioExiste(usuario.Username))
                return (false, "El nombre de usuario ya existe", null);

            if (await EmailExiste(usuario.Email))
                return (false, "El correo electrónico ya está registrado", null);

            // Verificar si estamos usando una base de datos en memoria (para pruebas)
            bool isInMemoryDatabase = _context.Database.ProviderName?.Contains("InMemory") == true;

            try
            {
                if (!isInMemoryDatabase)
                {
                    // En una base de datos real, usamos transacción
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    
                    try
                    {
                        await CompletarRegistroUsuarioEstudiante(usuario, estudiante, password);
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return (false, $"Error al registrar: {ex.Message}", null);
                    }
                }
                else
                {
                    // En base de datos en memoria (para pruebas), sin transacción
                    await CompletarRegistroUsuarioEstudiante(usuario, estudiante, password);
                }
                
                return (true, "Usuario y estudiante registrados correctamente", usuario);
            }
            catch (Exception ex)
            {
                return (false, $"Error al registrar: {ex.Message}", null);
            }
        }

        // Método auxiliar para evitar duplicación de código
        private async Task CompletarRegistroUsuarioEstudiante(Usuario usuario, Estudiante estudiante, string password)
        {
            // Guardar el estudiante primero
            _context.Estudiantes.Add(estudiante);
            await _context.SaveChangesAsync();
            
            // Preparar el usuario
            usuario.EstudianteId = estudiante.Id;
            usuario.Estudiante = estudiante;
            usuario.Password = password; // En producción: aplicar hash
            usuario.EsAdministrador = false; // Es una cuenta de estudiante
            usuario.FechaRegistro = DateTime.Now;
            
            // Guardar el usuario
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
        }
    }
}