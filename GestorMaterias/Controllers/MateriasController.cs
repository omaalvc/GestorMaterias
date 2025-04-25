using GestorMaterias.Data;
using GestorMaterias.Models;
using GestorMaterias.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GestorMaterias.Controllers
{
    public class MateriasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMateriaService _materiaService;

        public MateriasController(ApplicationDbContext context, IMateriaService materiaService)
        {
            _context = context;
            _materiaService = materiaService;
        }

        // GET: Materias
        public async Task<IActionResult> Index()
        {
            return View(await _materiaService.ObtenerTodasLasMaterias());
        }

        // GET: Materias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var materia = await _materiaService.ObtenerMateriaPorId(id.Value);
            if (materia == null)
            {
                return NotFound();
            }

            // Obtener estudiantes inscritos en la materia
            var registros = await _context.Registros
                .Include(r => r.Estudiante)
                .Where(r => r.MateriaId == id)
                .ToListAsync();

            ViewBag.Estudiantes = registros.Select(r => r.Estudiante).ToList();

            return View(materia);
        }

        // GET: Materias/Create
        public async Task<IActionResult> Create()
        {
            // Obtener solo profesores que pueden impartir más materias (menos de 2)
            var profesores = await _context.Profesores.ToListAsync();
            var profesoresDisponibles = new List<Profesor>();
            
            foreach (var profesor in profesores)
            {
                if (await _materiaService.ProfesorPuedeImpartirMasMateria(profesor.Id))
                {
                    profesoresDisponibles.Add(profesor);
                }
            }
            
            // Si no hay profesores disponibles, agregar un mensaje de advertencia
            if (profesoresDisponibles.Count == 0)
            {
                TempData["MensajeError"] = "No hay profesores disponibles para asignar a la materia. Cada profesor solo puede impartir un máximo de 2 materias.";
            }
            
            ViewBag.Profesores = new SelectList(profesoresDisponibles, "Id", "Nombre");
            return View();
        }

        // POST: Materias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Materia materia)
        {
            // Validación manual simplificada
            bool isValid = true;
            
            if (string.IsNullOrEmpty(materia.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre de la materia es requerido");
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(materia.Descripcion))
            {
                ModelState.AddModelError("Descripcion", "La descripción de la materia es requerida");
                isValid = false;
            }
            
            if (materia.ProfesorId <= 0)
            {
                ModelState.AddModelError("ProfesorId", "Debe seleccionar un profesor");
                isValid = false;
            }

            if (isValid)
            {
                var resultado = await _materiaService.CrearMateria(materia);
                if (resultado.Success)
                {
                    TempData["Mensaje"] = resultado.Message;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, resultado.Message);
                }
            }

            // Recargar lista de profesores
            var profesores = await _context.Profesores.ToListAsync();
            var profesoresDisponibles = new List<Profesor>();
            
            foreach (var profesor in profesores)
            {
                if (await _materiaService.ProfesorPuedeImpartirMasMateria(profesor.Id))
                {
                    profesoresDisponibles.Add(profesor);
                }
            }
            
            ViewBag.Profesores = new SelectList(profesoresDisponibles, "Id", "Nombre", materia.ProfesorId);
            return View(materia);
        }

        // GET: Materias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var materia = await _materiaService.ObtenerMateriaPorId(id.Value);
            if (materia == null)
            {
                return NotFound();
            }

            // Obtener profesores para el dropdown
            var profesores = await _context.Profesores.ToListAsync();
            var profesoresDisponibles = new List<Profesor>();
            
            // Incluir el profesor actual de la materia
            profesoresDisponibles.Add(materia.Profesor);
            
            // Añadir otros profesores que puedan impartir más materias
            foreach (var profesor in profesores)
            {
                if (profesor.Id != materia.ProfesorId && await _materiaService.ProfesorPuedeImpartirMasMateria(profesor.Id))
                {
                    profesoresDisponibles.Add(profesor);
                }
            }
            
            ViewBag.Profesores = new SelectList(profesoresDisponibles.Distinct(), "Id", "Nombre", materia.ProfesorId);
            return View(materia);
        }

        // POST: Materias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion,ProfesorId")] Materia materia)
        {
            if (id != materia.Id)
            {
                return NotFound();
            }

            try
            {
                // Obtener la materia existente para actualizarla
                var materiaExistente = await _context.Materias.FindAsync(id);
                
                if (materiaExistente == null)
                {
                    return NotFound();
                }

                // Actualizar los campos
                materiaExistente.Nombre = materia.Nombre;
                materiaExistente.Descripcion = materia.Descripcion;
                materiaExistente.ProfesorId = materia.ProfesorId;

                // Guardar cambios
                await _context.SaveChangesAsync();
                
                TempData["Mensaje"] = "Materia actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al actualizar: {ex.Message}");
            }

            // Recargar profesores en caso de error
            var profesores = await _context.Profesores.ToListAsync();
            var profesoresDisponibles = new List<Profesor>();
            
            // Incluir el profesor actual
            var profesorActual = await _context.Profesores.FindAsync(materia.ProfesorId);
            if (profesorActual != null)
            {
                profesoresDisponibles.Add(profesorActual);
            }
            
            // Añadir otros profesores que pueden impartir más materias
            foreach (var profesor in profesores)
            {
                if (profesor.Id != materia.ProfesorId && await _materiaService.ProfesorPuedeImpartirMasMateria(profesor.Id))
                {
                    profesoresDisponibles.Add(profesor);
                }
            }
            
            ViewBag.Profesores = new SelectList(profesoresDisponibles.Distinct(), "Id", "Nombre", materia.ProfesorId);
            return View(materia);
        }

        // GET: Materias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var materia = await _context.Materias
                .Include(m => m.Profesor)
                .Include(m => m.Registros)
                .ThenInclude(r => r.Estudiante)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (materia == null)
            {
                return NotFound();
            }

            // Para la vista necesitamos obtener los estudiantes de la materia
            //materia.Estudiantes = materia.Registros?.Select(r => r.Estudiante).ToList() ?? new List<Estudiante>();

            return View(materia);
        }

        // POST: Materias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var materia = await _context.Materias
                .Include(m => m.Profesor)
                .Include(m => m.Registros)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (materia == null)
            {
                return NotFound();
            }

            // Verificar si la materia tiene estudiantes matriculados o profesor asignado
            if ((materia.Registros != null && materia.Registros.Any()) || materia.Profesor != null)
            {
                ViewBag.Error = "No se puede eliminar la materia porque tiene estudiantes matriculados o un profesor asignado.";
                
                // Recargar la materia con sus estudiantes para la vista
                materia = await _context.Materias
                    .Include(m => m.Profesor)
                    .Include(m => m.Registros)
                    .ThenInclude(r => r.Estudiante)
                    .FirstOrDefaultAsync(m => m.Id == id);
                    
                //materia.Estudiantes = materia.Registros?.Select(r => r.Estudiante).ToList() ?? new List<Estudiante>();
                
                return View(materia);
            }

            _context.Materias.Remove(materia);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}