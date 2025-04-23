using GestorMaterias.Data;
using GestorMaterias.Models;
using Microsoft.EntityFrameworkCore;

namespace GestorMaterias.Services
{
    public class RegistroService : IRegistroService
    {
        private readonly ApplicationDbContext _context;

        public RegistroService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Verifica si un estudiante puede inscribirse a una materia
        public async Task<bool> PuedeInscribirseAMateria(int estudianteId, int materiaId)
        {
            // Obtener estudiante con sus registros
            var estudiante = await _context.Estudiantes
                .Include(e => e.Registros)
                    .ThenInclude(r => r.Materia)
                        .ThenInclude(m => m.Profesor)
                .FirstOrDefaultAsync(e => e.Id == estudianteId);

            if (estudiante == null)
                return false;

            // Verificar si ya está inscrito en 3 materias
            if (estudiante.Registros.Count >= 3)
                return false;

            // Obtener la materia con su profesor
            var materia = await _context.Materias
                .Include(m => m.Profesor)
                .FirstOrDefaultAsync(m => m.Id == materiaId);

            if (materia == null)
                return false;

            // Verificar si ya está inscrito en esta materia
            if (estudiante.Registros.Any(r => r.MateriaId == materiaId))
                return false;

            // Verificar si ya tiene una materia con el mismo profesor
            if (estudiante.Registros.Any(r => r.Materia.ProfesorId == materia.ProfesorId))
                return false;

            return true;
        }

        // Inscribe un estudiante a una materia si cumple las condiciones
        public async Task<(bool Success, string Message)> InscribirEstudianteAMateria(int estudianteId, int materiaId)
        {
            // Primero verificar si puede inscribirse
            if (!await PuedeInscribirseAMateria(estudianteId, materiaId))
            {
                var materias = await ObtenerMateriasInscritasEstudiante(estudianteId);
                
                if (materias.Count >= 3)
                    return (false, "El estudiante ya tiene el máximo de 3 materias inscritas.");
                
                if (materias.Any(m => m.Id == materiaId))
                    return (false, "El estudiante ya está inscrito en esta materia.");
                
                var materia = await _context.Materias
                    .Include(m => m.Profesor)
                    .FirstOrDefaultAsync(m => m.Id == materiaId);

                if (await EstudianteTieneMismoProfesor(estudianteId, materia.ProfesorId))
                    return (false, "El estudiante ya tiene una materia con el mismo profesor.");
                
                return (false, "No se pudo realizar la inscripción.");
            }

            // Crear el nuevo registro
            var registro = new Registro
            {
                EstudianteId = estudianteId,
                MateriaId = materiaId,
                FechaInscripcion = DateTime.Now,
                Estado = "Activo"
            };

            _context.Registros.Add(registro);
            await _context.SaveChangesAsync();

            return (true, "Inscripción realizada correctamente.");
        }

        // Obtiene las materias inscritas por un estudiante
        public async Task<List<Materia>> ObtenerMateriasInscritasEstudiante(int estudianteId)
        {
            return await _context.Registros
                .Where(r => r.EstudianteId == estudianteId)
                .Select(r => r.Materia)
                .ToListAsync();
        }

        // Obtiene los estudiantes inscritos en una materia
        public async Task<List<Estudiante>> ObtenerEstudiantesPorMateria(int materiaId)
        {
            return await _context.Registros
                .Where(r => r.MateriaId == materiaId)
                .Select(r => r.Estudiante)
                .ToListAsync();
        }

        // Obtiene los registros (inscripciones) de un estudiante
        public async Task<List<Registro>> ObtenerRegistrosPorEstudiante(int estudianteId)
        {
            return await _context.Registros
                .Where(r => r.EstudianteId == estudianteId)
                .Include(r => r.Materia)
                    .ThenInclude(m => m.Profesor)
                .ToListAsync();
        }

        // Elimina una inscripción existente
        public async Task<(bool Success, string Message)> EliminarInscripcion(int registroId)
        {
            var registro = await _context.Registros.FindAsync(registroId);
            if (registro == null)
                return (false, "Inscripción no encontrada.");

            _context.Registros.Remove(registro);
            await _context.SaveChangesAsync();

            return (true, "Inscripción eliminada correctamente.");
        }

        // Verifica si un estudiante ya tiene materias con un profesor específico
        public async Task<bool> EstudianteTieneMismoProfesor(int estudianteId, int profesorId)
        {
            return await _context.Registros
                .Where(r => r.EstudianteId == estudianteId)
                .Include(r => r.Materia)
                .AnyAsync(r => r.Materia.ProfesorId == profesorId);
        }
    }
}