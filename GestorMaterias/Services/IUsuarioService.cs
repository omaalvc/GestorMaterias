using GestorMaterias.Models;

namespace GestorMaterias.Services
{
    public interface IUsuarioService
    {
        Task<(bool success, string message, Usuario? usuario)> ValidarCredenciales(string username, string password);
        Task<(bool success, string message, Usuario? usuario)> RegistrarUsuario(Usuario usuario, string password, bool isStudentAccount = true);
        Task<Usuario?> ObtenerUsuarioPorId(int id);
        Task<Usuario?> ObtenerUsuarioPorUsername(string username);
        Task<bool> AsociarEstudianteAUsuario(int usuarioId, int estudianteId);
        Task<bool> UsuarioExiste(string username);
        Task<bool> EmailExiste(string email);
        Task<(bool success, string message, Usuario? usuario)> RegistrarUsuarioYEstudiante(Usuario usuario, Estudiante estudiante, string password);
    }
}