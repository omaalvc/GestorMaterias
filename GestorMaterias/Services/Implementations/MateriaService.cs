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
                .AsNoTracking()  // Mejora el rendimiento para consultas de solo lectura
                .ToListAsync();
        }

        public async Task<Materia> ObtenerMateriaPorId(int id)
        {
            return await _context.Materias
                .Include(m => m.Profesor)
                .Include(m => m.Registros.Where(r => r.Estado == "Activo"))
                    .ThenInclude(r => r.Estudiante)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        // Este método devuelve las materias con información básica del profesor pero sin incluir
        // las referencias cíclicas que causan problemas de serialización
        public async Task<List<object>> ObtenerMateriasParaAPI()
        {
            var materias = await _context.Materias
                .Include(m => m.Profesor)
                .Include(m => m.Registros.Where(r => r.Estado == "Activo"))
                .AsNoTracking()
                .ToListAsync();
                
            return materias.Select(m => new
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Descripcion = m.Descripcion,
                Creditos = m.Creditos,
                Profesor = m.Profesor != null ? new
                {
                    Id = m.Profesor.Id,
                    Nombre = m.Profesor.Nombre,
                    //Apellido = m.Profesor.Apellido
                } : null,
                CantidadEstudiantes = m.Registros?.Count(r => r.Estado == "Activo") ?? 0
            }).ToList<object>();
        }

        // Este método devuelve una materia específica con información detallada pero sin referencias cíclicas
        public async Task<object> ObtenerMateriaPorIdParaAPI(int id)
        {
            var materia = await _context.Materias
                .Include(m => m.Profesor)
                .Include(m => m.Registros.Where(r => r.Estado == "Activo"))
                    .ThenInclude(r => r.Estudiante)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (materia == null)
                return null;

            return new
            {
                Id = materia.Id,
                Nombre = materia.Nombre,
                Descripcion = materia.Descripcion,
                Creditos = materia.Creditos,
                Profesor = materia.Profesor != null ? new
                {
                    Id = materia.Profesor.Id,
                    Nombre = materia.Profesor.Nombre,
                    //Apellido = materia.Profesor.Apellido
                } : null,
                Estudiantes = materia.Registros
                    .Where(r => r.Estado == "Activo")
                    .Select(r => new
                    {
                        Id = r.EstudianteId,
                        Nombre = r.Estudiante?.Nombre,
                        //Apellido = r.Estudiante?.Apellido,
                        //FechaRegistro = r.FechaRegistro
                    }).ToList()
            };
        }

        public async Task<List<Materia>> ObtenerMateriasPorProfesor(int profesorId)
        {
            return await _context.Materias
                .Where(m => m.ProfesorId == profesorId)
                .Include(m => m.Registros)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> CrearMateria(Materia materia)
        {
            try
            {
                // Verificar si el nombre de materia ya existe
                if (await _context.Materias.AnyAsync(m => m.Nombre.ToLower() == materia.Nombre.ToLower()))
                {
                    return (false, "Ya existe una materia con ese nombre");
                }

                // Verificar si el profesor puede impartir otra materia
                if (materia.ProfesorId > 0 && !await ProfesorPuedeImpartirMasMateria(materia.ProfesorId))
                {
                    return (false, "El profesor ya tiene el máximo de materias permitidas");
                }

                // Asignar créditos predeterminados
                materia.Creditos = 3;

                _context.Materias.Add(materia);
                await _context.SaveChangesAsync();
                return (true, "Materia creada con éxito");
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear la materia: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ActualizarMateria(Materia materia)
        {
            try
            {
                // Verificar si la materia existe
                var materiaExistente = await _context.Materias.FindAsync(materia.Id);
                if (materiaExistente == null)
                {
                    return (false, "La materia no existe");
                }

                // Verificar duplicados de nombre si el nombre ha cambiado
                if (materiaExistente.Nombre != materia.Nombre && 
                    await _context.Materias.AnyAsync(m => m.Nombre.ToLower() == materia.Nombre.ToLower() && m.Id != materia.Id))
                {
                    return (false, "Ya existe otra materia con ese nombre");
                }

                // Verificar si el profesor puede impartir otra materia (si ha cambiado de profesor)
                if (materia.ProfesorId > 0 && 
                    materiaExistente.ProfesorId != materia.ProfesorId && 
                    !await ProfesorPuedeImpartirMasMateria(materia.ProfesorId))
                {
                    return (false, "El profesor ya tiene el máximo de materias permitidas");
                }

                // Actualizar propiedades
                materiaExistente.Nombre = materia.Nombre;
                materiaExistente.Descripcion = materia.Descripcion;
                materiaExistente.ProfesorId = materia.ProfesorId;
                // Los créditos siempre son 3

                await _context.SaveChangesAsync();
                return (true, "Materia actualizada con éxito");
            }
            catch (Exception ex)
            {
                return (false, $"Error al actualizar la materia: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> EliminarMateria(int id)
        {
            try
            {
                var materia = await _context.Materias
                    .Include(m => m.Registros)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (materia == null)
                {
                    return (false, "La materia no existe");
                }

                // Verificar si hay estudiantes inscritos
                if (materia.Registros != null && materia.Registros.Any(r => r.Estado == "Activo"))
                {
                    return (false, "No se puede eliminar la materia porque tiene estudiantes inscritos");
                }

                _context.Materias.Remove(materia);
                await _context.SaveChangesAsync();
                return (true, "Materia eliminada con éxito");
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar la materia: {ex.Message}");
            }
        }

        public async Task<bool> ProfesorPuedeImpartirMasMateria(int profesorId)
        {
            // Verificar si existe el profesor
            var profesor = await _context.Profesores.FindAsync(profesorId);
            if (profesor == null)
            {
                return false;
            }

            // Contar cuántas materias imparte actualmente
            var cantidadMaterias = await _context.Materias
                .CountAsync(m => m.ProfesorId == profesorId);

            // Un profesor puede impartir máximo 2 materias
            return cantidadMaterias < 2;
        }
    }
}