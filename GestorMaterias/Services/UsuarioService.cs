using GestorMaterias.Data;
using GestorMaterias.Models;
using Microsoft.EntityFrameworkCore;

namespace GestorMaterias.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly ApplicationDbContext _context;

        public UsuarioService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool success, string message, Usuario? usuario)> ValidarCredenciales(string username, string password)
        {
            // En un sistema real, usaríamos hashing para las contraseñas
            var usuario = await _context.Usuarios
                .Include(u => u.Estudiante)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (usuario == null)
                return (false, "Usuario no encontrado", null);

            if (usuario.Password != password) // En producción: verificar hash
                return (false, "Contraseña incorrecta", null);

            return (true, "Inicio de sesión exitoso", usuario);
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
            usuario.Password = password; // Almacenar directamente para este ejemplo
            usuario.EsAdministrador = !isStudentAccount; // Por defecto, los nuevos usuarios son estudiantes

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return (true, "Usuario registrado correctamente", usuario);
        }

        public async Task<Usuario?> ObtenerUsuarioPorId(int id)
        {
            return await _context.Usuarios
                .Include(u => u.Estudiante)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Usuario?> ObtenerUsuarioPorUsername(string username)
        {
            return await _context.Usuarios
                .Include(u => u.Estudiante)
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
    }
}