using GestorMaterias.Models;

namespace GestorMaterias.Services
{
    public interface IRegistroService
    {
        Task<bool> PuedeInscribirseAMateria(int estudianteId, int materiaId);
        Task<(bool Success, string Message)> InscribirEstudianteAMateria(int estudianteId, int materiaId);
        Task<List<Materia>> ObtenerMateriasInscritasEstudiante(int estudianteId);
        Task<List<Estudiante>> ObtenerEstudiantesPorMateria(int materiaId);
        Task<List<Registro>> ObtenerRegistrosPorEstudiante(int estudianteId);
        Task<(bool Success, string Message)> EliminarInscripcion(int registroId);
        Task<bool> EstudianteTieneMismoProfesor(int estudianteId, int profesorId);
        Task<List<Registro>> ObtenerTodosLosRegistros();
        Task<OperationResult> InscribirEstudianteEnMateria(int estudianteId, int materiaId);
        Task<OperationResult> CancelarInscripcion(int registroId);
        Task<List<Materia>> ObtenerMateriasDisponiblesParaEstudiante(int estudianteId);
    }
}