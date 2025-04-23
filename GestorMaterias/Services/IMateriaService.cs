using GestorMaterias.Models;

namespace GestorMaterias.Services
{
    public interface IMateriaService
    {
        Task<List<Materia>> ObtenerTodasLasMaterias();
        Task<Materia> ObtenerMateriaPorId(int id);
        Task<List<Materia>> ObtenerMateriasPorProfesor(int profesorId);
        Task<(bool Success, string Message)> CrearMateria(Materia materia);
        Task<(bool Success, string Message)> ActualizarMateria(Materia materia);
        Task<(bool Success, string Message)> EliminarMateria(int id);
        Task<bool> ProfesorPuedeImpartirMasMateria(int profesorId);
    }
}