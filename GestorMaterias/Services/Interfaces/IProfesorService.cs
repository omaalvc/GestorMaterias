using GestorMaterias.Models;

namespace GestorMaterias.Services
{
    public interface IProfesorService
    {
        Task<IEnumerable<Profesor>> ObtenerTodosProfesores();
        Task<Profesor?> ObtenerProfesorPorId(int id);
        Task<Profesor?> CrearProfesor(Profesor profesor);
        Task<Profesor?> ActualizarProfesor(Profesor profesor);
        Task<bool> EliminarProfesor(int id);
    }
}