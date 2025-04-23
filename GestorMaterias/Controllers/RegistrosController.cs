using GestorMaterias.Data;
using GestorMaterias.Models;
using GestorMaterias.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GestorMaterias.Controllers
{
    public class RegistrosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRegistroService _registroService;
        private readonly IMateriaService _materiaService;

        public RegistrosController(
            ApplicationDbContext context, 
            IRegistroService registroService,
            IMateriaService materiaService)
        {
            _context = context;
            _registroService = registroService;
            _materiaService = materiaService;
        }

        // GET: Registros
        public async Task<IActionResult> Index()
        {
            var registros = await _context.Registros
                .Include(r => r.Estudiante)
                .Include(r => r.Materia)
                    .ThenInclude(m => m.Profesor)
                .ToListAsync();
            return View(registros);
        }

        // GET: Registros/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registro = await _context.Registros
                .Include(r => r.Estudiante)
                .Include(r => r.Materia)
                    .ThenInclude(m => m.Profesor)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (registro == null)
            {
                return NotFound();
            }

            return View(registro);
        }

        // GET: Registros/Create
        public async Task<IActionResult> Create()
        {
            // Cargar listas desplegables
            ViewBag.Estudiantes = new SelectList(await _context.Estudiantes.ToListAsync(), "Id", "Nombre");
            ViewBag.Materias = new SelectList(await _context.Materias
                .Include(m => m.Profesor)
                .Select(m => new { 
                    Id = m.Id, 
                    NombreCompleto = $"{m.Nombre} - Prof. {m.Profesor.Nombre}" 
                })
                .ToListAsync(), "Id", "NombreCompleto");
            
            return View();
        }

        // POST: Registros/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EstudianteId,MateriaId")] Registro registro)
        {
            if (ModelState.IsValid)
            {
                // Utilizar el servicio para realizar la inscripción con validaciones
                var resultado = await _registroService.InscribirEstudianteAMateria(
                    registro.EstudianteId, registro.MateriaId);

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

            // Recargar las listas en caso de error
            ViewBag.Estudiantes = new SelectList(await _context.Estudiantes.ToListAsync(), "Id", "Nombre", registro.EstudianteId);
            ViewBag.Materias = new SelectList(await _context.Materias
                .Include(m => m.Profesor)
                .Select(m => new { 
                    Id = m.Id, 
                    NombreCompleto = $"{m.Nombre} - Prof. {m.Profesor.Nombre}" 
                })
                .ToListAsync(), "Id", "NombreCompleto", registro.MateriaId);
                
            return View(registro);
        }

        // GET: Registros/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registro = await _context.Registros
                .Include(r => r.Estudiante)
                .Include(r => r.Materia)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (registro == null)
            {
                return NotFound();
            }

            return View(registro);
        }

        // POST: Registros/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var resultado = await _registroService.EliminarInscripcion(id);
            
            if (resultado.Success)
            {
                TempData["Mensaje"] = resultado.Message;
            }
            else
            {
                TempData["Error"] = resultado.Message;
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Registros/PorEstudiante/5
        public async Task<IActionResult> PorEstudiante(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estudiante = await _context.Estudiantes.FindAsync(id);
            if (estudiante == null)
            {
                return NotFound();
            }

            // Obtener registros del estudiante
            var registros = await _registroService.ObtenerRegistrosPorEstudiante(id.Value);
            
            ViewBag.Estudiante = estudiante;
            return View(registros);
        }
        
        // GET: Registros/MateriasDisponibles/5
        public async Task<IActionResult> MateriasDisponibles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estudiante = await _context.Estudiantes.FindAsync(id);
            if (estudiante == null)
            {
                return NotFound();
            }

            // Obtener todas las materias
            var todasLasMaterias = await _materiaService.ObtenerTodasLasMaterias();
            
            // Obtener materias inscritas por el estudiante
            var materiasInscritas = await _registroService.ObtenerMateriasInscritasEstudiante(id.Value);
            
            // Obtener profesores con los que ya tiene materias
            var profesoresActuales = materiasInscritas
                .Select(m => m.ProfesorId)
                .ToList();

            // Filtrar materias disponibles (no inscritas y que no tenga al mismo profesor)
            var materiasDisponibles = todasLasMaterias
                .Where(m => !materiasInscritas.Any(mi => mi.Id == m.Id) && 
                           !profesoresActuales.Contains(m.ProfesorId))
                .ToList();

            ViewBag.Estudiante = estudiante;
            return View(materiasDisponibles);
        }
        
        // POST: Registros/Inscribir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribir(int estudianteId, int materiaId)
        {
            var resultado = await _registroService.InscribirEstudianteAMateria(estudianteId, materiaId);
            
            if (resultado.Success)
            {
                TempData["Mensaje"] = resultado.Message;
            }
            else
            {
                TempData["Error"] = resultado.Message;
            }
            
            return RedirectToAction("Details", "Estudiantes", new { id = estudianteId });
        }
    }
}