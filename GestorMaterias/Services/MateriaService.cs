using GestorMaterias.Data;
using GestorMaterias.Models;
using Microsoft.EntityFrameworkCore;

namespace GestorMaterias.Services
{
    public class MateriaService : IMateriaService
    {
        private readonly ApplicationDbContext _context;

        public MateriaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Materia>> ObtenerTodasLasMaterias()
        {
            return await _context.Materias
                .Include(m => m.Profesor)
                .ToListAsync();
        }

        public async Task<Materia> ObtenerMateriaPorId(int id)
        {
            return await _context.Materias
                .Include(m => m.Profesor)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Materia>> ObtenerMateriasPorProfesor(int profesorId)
        {
            return await _context.Materias
                .Where(m => m.ProfesorId == profesorId)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> CrearMateria(Materia materia)
        {
            // Validar que el profesor pueda impartir otra materia
            if (!await ProfesorPuedeImpartirMasMateria(materia.ProfesorId))
                return (false, "El profesor ya imparte el máximo de 2 materias permitidas.");

            // Validar que la materia tenga 3 créditos según el requerimiento
            materia.Creditos = 3;

            _context.Materias.Add(materia);
            await _context.SaveChangesAsync();

            return (true, "Materia creada correctamente.");
        }

        public async Task<(bool Success, string Message)> ActualizarMateria(Materia materia)
        {
            var materiaExistente = await _context.Materias.FindAsync(materia.Id);
            if (materiaExistente == null)
                return (false, "Materia no encontrada.");

            // Si el profesor cambió, validar que el nuevo profesor pueda impartir otra materia
            if (materiaExistente.ProfesorId != materia.ProfesorId)
            {
                if (!await ProfesorPuedeImpartirMasMateria(materia.ProfesorId))
                    return (false, "El profesor ya imparte el máximo de 2 materias permitidas.");
            }

            // Mantener siempre 3 créditos según el requerimiento
            materia.Creditos = 3;

            materiaExistente.Nombre = materia.Nombre;
            materiaExistente.Descripcion = materia.Descripcion;
            materiaExistente.ProfesorId = materia.ProfesorId;
            
            await _context.SaveChangesAsync();

            return (true, "Materia actualizada correctamente.");
        }

        public async Task<(bool Success, string Message)> EliminarMateria(int id)
        {
            var materia = await _context.Materias
                .Include(m => m.Registros)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (materia == null)
                return (false, "Materia no encontrada.");

            // Verificar si hay estudiantes inscritos en la materia
            if (materia.Registros.Any())
                return (false, "No se puede eliminar la materia porque tiene estudiantes inscritos.");

            _context.Materias.Remove(materia);
            await _context.SaveChangesAsync();

            return (true, "Materia eliminada correctamente.");
        }

        public async Task<bool> ProfesorPuedeImpartirMasMateria(int profesorId)
        {
            // Un profesor solo puede impartir máximo 2 materias según los requerimientos
            var cantidadMaterias = await _context.Materias
                .CountAsync(m => m.ProfesorId == profesorId);

            return cantidadMaterias < 2;
        }
    }
}