using GestorMaterias.Models;

namespace GestorMaterias.Services
{
    public interface IRegistroService
    {
        Task<List<Registro>> ObtenerRegistrosPorEstudiante(int estudianteId);
        Task<bool> PuedeInscribirseAMateria(int estudianteId, int materiaId);
        Task<(bool Success, string Message)> InscribirEstudianteAMateria(int estudianteId, int materiaId);
        Task<List<Materia>> ObtenerMateriasInscritasEstudiante(int estudianteId);
        Task<List<Estudiante>> ObtenerEstudiantesPorMateria(int materiaId);
        Task<(bool Success, string Message)> EliminarInscripcion(int registroId);
        Task<bool> EstudianteTieneMismoProfesor(int estudianteId, int profesorId);
        Task<List<Registro>> ObtenerTodosLosRegistros();
        Task<OperationResult> InscribirEstudianteEnMateria(int estudianteId, int materiaId);
        Task<OperationResult> CancelarInscripcion(int registroId);
        Task<List<Materia>> ObtenerMateriasDisponiblesParaEstudiante(int estudianteId);
        Task<bool> MatricularEstudiante(int estudianteId, int materiaId);
        Task<bool> CancelarMatricula(int estudianteId, int materiaId);
        Task<List<Materia>> GetMateriasEstudiante(int estudianteId);
        Task<List<Materia>> GetMateriasDisponibles(int estudianteId);
    }
}