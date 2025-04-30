using GestorMaterias.Data;
using GestorMaterias.Models;
using Microsoft.EntityFrameworkCore;

namespace GestorMaterias.Services
{
    public class ProfesorService : IProfesorService
    {
        private readonly ApplicationDbContext _context;

        public ProfesorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Profesor>> ObtenerTodosProfesores()
        {
            return await _context.Profesores
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Profesor> ObtenerProfesorPorId(int id)
        {
            return await _context.Profesores
                //.Include(p => p.Usuario)
                .Include(p => p.Materias)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<(bool Success, string Message)> CrearProfesor(Profesor profesor)
        {
            try
            {
                // Verificar que no exista un profesor con el mismo nombre y apellido
                if (await _context.Profesores.AnyAsync(p => 
                    p.Nombre == profesor.Nombre ))
                {
                    return (false, "Ya existe un profesor con el mismo nombre y apellido");
                }

                _context.Profesores.Add(profesor);
                await _context.SaveChangesAsync();
                return (true, "Profesor creado con éxito");
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear profesor: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ActualizarProfesor(Profesor profesor)
        {
            try
            {
                var profesorExistente = await _context.Profesores.FindAsync(profesor.Id);
                if (profesorExistente == null)
                {
                    return (false, "Profesor no encontrado");
                }

                // Verificar que no exista otro profesor con el mismo nombre y apellido
                if (await _context.Profesores.AnyAsync(p => 
                    p.Id != profesor.Id && p.Nombre == profesor.Nombre ))
                {
                    return (false, "Ya existe otro profesor con el mismo nombre y apellido");
                }

                profesorExistente.Nombre = profesor.Nombre;
                //profesorExistente.Apellido = profesor.Apellido;
                profesorExistente.Email = profesor.Email;
                //profesorExistente.Telefono = profesor.Telefono;
                //profesorExistente.Especialidad = profesor.Especialidad;

                await _context.SaveChangesAsync();
                return (true, "Profesor actualizado con éxito");
            }
            catch (Exception ex)
            {
                return (false, $"Error al actualizar profesor: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> EliminarProfesor(int id)
        {
            try
            {
                var profesor = await _context.Profesores
                    .Include(p => p.Materias)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (profesor == null)
                {
                    return (false, "Profesor no encontrado");
                }

                // Verificar si el profesor tiene materias asociadas
                if (profesor.Materias != null && profesor.Materias.Any())
                {
                    return (false, "No se puede eliminar el profesor porque tiene materias asignadas");
                }

                _context.Profesores.Remove(profesor);
                await _context.SaveChangesAsync();
                return (true, "Profesor eliminado con éxito");
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar profesor: {ex.Message}");
            }
        }

        public async Task<bool> ProfesorExiste(int id)
        {
            return await _context.Profesores.AnyAsync(p => p.Id == id);
        }

        public async Task<int> ContarMateriasPorProfesor(int profesorId)
        {
            return await _context.Materias.CountAsync(m => m.ProfesorId == profesorId);
        }

        public async Task<Profesor> ObtenerProfesorPorUsuarioId(int usuarioId)
        {
            return await _context.Profesores
                .Include(p => p.Materias)
                .FirstOrDefaultAsync(p => p.Id == usuarioId);
        }

        public async Task<List<Profesor>> ObtenerProfesoresDisponibles()
        {
            // Obtenemos todos los profesores que tienen menos de 2 materias asignadas
            var profesores = await _context.Profesores
                .Include(p => p.Materias)
                .AsNoTracking()
                .ToListAsync();

            return profesores.Where(p => p.Materias == null || p.Materias.Count < 2).ToList();
        }

        Task<IEnumerable<Profesor>> IProfesorService.ObtenerTodosProfesores()
        {
            throw new NotImplementedException();
        }

        Task<Profesor?> IProfesorService.CrearProfesor(Profesor profesor)
        {
            throw new NotImplementedException();
        }

        Task<Profesor?> IProfesorService.ActualizarProfesor(Profesor profesor)
        {
            throw new NotImplementedException();
        }

        Task<bool> IProfesorService.EliminarProfesor(int id)
        {
            throw new NotImplementedException();
        }
    }
}