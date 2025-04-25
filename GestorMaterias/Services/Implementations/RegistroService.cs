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
                .Where(r => r.EstudianteId == estudianteId && r.Estado == "Activo")
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

        public async Task<List<Registro>> ObtenerTodosLosRegistros()
        {
            return await _context.Registros
                .Include(r => r.Estudiante)
                .Include(r => r.Materia)
                    .ThenInclude(m => m.Profesor)
                .ToListAsync();
        }

                // Verifica si un estudiante ya tiene materias con un profesor específico
        public async Task<bool> EstudianteTieneMismoProfesor(int estudianteId, int profesorId)
        {
            return await _context.Registros
                .Where(r => r.EstudianteId == estudianteId)
                .Include(r => r.Materia)
                .AnyAsync(r => r.Materia.ProfesorId == profesorId);
        }

        public async Task<OperationResult> InscribirEstudianteEnMateria(int estudianteId, int materiaId)
        {
            try
            {
                // Verificar si el estudiante existe
                var estudiante = await _context.Estudiantes.FindAsync(estudianteId);
                if (estudiante == null)
                {
                    return new OperationResult { Success = false, Message = "El estudiante no existe" };
                }

                // Verificar si la materia existe
                var materia = await _context.Materias
                    .Include(m => m.Profesor)
                    .FirstOrDefaultAsync(m => m.Id == materiaId);
                
                if (materia == null)
                {
                    return new OperationResult { Success = false, Message = "La materia no existe" };
                }

                // Verificar si ya está inscrito en esta materia
                var registroExistente = await _context.Registros
                    .AnyAsync(r => r.EstudianteId == estudianteId && r.MateriaId == materiaId && r.Estado == "Activo");
                
                if (registroExistente)
                {
                    return new OperationResult { Success = false, Message = "El estudiante ya está inscrito en esta materia" };
                }

                // Verificar cuántas materias tiene inscritas el estudiante
                var materiasInscritas = await _context.Registros
                    .CountAsync(r => r.EstudianteId == estudianteId && r.Estado == "Activo");
                
                if (materiasInscritas >= 3)
                {
                    return new OperationResult { Success = false, Message = "El estudiante ya tiene el máximo de materias permitidas (3)" };
                }

                // Verificar si ya tiene una materia con este profesor
                if (materia.ProfesorId > 0)
                {
                    var yaInscritoConProfesor = await _context.Registros
                        .Include(r => r.Materia)
                        .AnyAsync(r => r.EstudianteId == estudianteId && 
                                        r.Estado == "Activo" && 
                                        r.Materia.ProfesorId == materia.ProfesorId);
                    
                    if (yaInscritoConProfesor)
                    {
                        return new OperationResult { Success = false, Message = "El estudiante ya tiene una materia con este profesor" };
                    }
                }

                // Crear nuevo registro
                var nuevoRegistro = new Registro
                {
                    EstudianteId = estudianteId,
                    MateriaId = materiaId,
                    FechaInscripcion = DateTime.Now,
                    Estado = "Activo"
                };

                _context.Registros.Add(nuevoRegistro);
                await _context.SaveChangesAsync();

                return new OperationResult { Success = true, Message = $"Inscripción exitosa en {materia.Nombre}" };
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Message = $"Error al procesar la inscripción: {ex.Message}" };
            }
        }

        public async Task<OperationResult> CancelarInscripcion(int registroId)
        {
            try
            {
                var registro = await _context.Registros
                    .Include(r => r.Materia)
                    .FirstOrDefaultAsync(r => r.Id == registroId);

                if (registro == null)
                {
                    return new OperationResult { Success = false, Message = "El registro no existe" };
                }

                // Cambiar estado a cancelado
                registro.Estado = "Cancelado";

                await _context.SaveChangesAsync();
                return new OperationResult { Success = true, Message = $"Inscripción a {registro.Materia.Nombre} cancelada con éxito" };
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Message = $"Error al cancelar la inscripción: {ex.Message}" };
            }
        }

        public async Task<List<Materia>> ObtenerMateriasDisponiblesParaEstudiante(int estudianteId)
        {
            // Obtener estudiante
            var estudiante = await _context.Estudiantes.FindAsync(estudianteId);
            if (estudiante == null)
            {
                return new List<Materia>();
            }

            // Obtener las materias en las que ya está inscrito
            var materiasInscritas = await _context.Registros
                .Where(r => r.EstudianteId == estudianteId && r.Estado == "Activo")
                .Select(r => r.MateriaId)
                .ToListAsync();

            // Obtener los profesores con los que ya tiene materias
            var profesoresConMaterias = await _context.Registros
                .Where(r => r.EstudianteId == estudianteId && r.Estado == "Activo")
                .Join(_context.Materias.Where(m => m.ProfesorId != null),
                      r => r.MateriaId,
                      m => m.Id,
                      (r, m) => m.ProfesorId)
                .ToListAsync();

            // Obtener todas las materias disponibles
            var materiasDisponibles = await _context.Materias
                .Include(m => m.Profesor)
                .Include(m => m.Registros)
                .Where(m => !materiasInscritas.Contains(m.Id) && 
                           (m.ProfesorId == null || !profesoresConMaterias.Contains(m.ProfesorId)))
                .ToListAsync();

            return materiasDisponibles;
        }
    }

    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}