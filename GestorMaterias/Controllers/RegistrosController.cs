using GestorMaterias.Data;
using GestorMaterias.Models;
using GestorMaterias.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            int estudianteId;
            if (id.HasValue)
            {
                estudianteId = id.Value;
            }
            else
            {
                // Obtener el id del estudiante autenticado
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EstudianteId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out estudianteId))
                {
                    TempData["Error"] = "No se pudo identificar al estudiante.";
                    return RedirectToAction("Index", "Home");
                }
            }

            // Utilizar el servicio para obtener los registros del estudiante
            var inscripciones = await _registroService.ObtenerRegistrosPorEstudiante(estudianteId);

            return View(inscripciones);
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

        // GET: Registros/MateriasDisponiblesEstudiantes/2
        public async Task<IActionResult> MateriasDisponiblesEstudiantes(int? id)
        {
            int estudianteId;
            
            if (id.HasValue)
            {
                // If an ID is provided in the URL, use that
                estudianteId = id.Value;
            }
            else
            {
                // Otherwise get the ID from the authenticated user
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EstudianteId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out estudianteId))
                {
                    TempData["Error"] = "No se pudo identificar al estudiante.";
                    return RedirectToAction("Index", "Home");
                }
            }

            // Verificar que el estudiante existe
            var estudiante = await _context.Estudiantes.FindAsync(estudianteId);
            if (estudiante == null)
            {
                return NotFound();
            }

            // Obtener materias disponibles
            var materiasDisponibles = await _context.Materias
                .Include(m => m.Profesor)
                .Where(m => !_context.Registros.Any(r => r.EstudianteId == estudianteId && r.MateriaId == m.Id && r.Estado == "Activo"))
                .ToListAsync();

            ViewBag.Estudiante = estudiante;
            
            return View("MateriasDisponibles", materiasDisponibles);
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

        // GET: Registros/InscribirMateria?estudianteId={estudianteId}&materiaId={materiaId}
        public async Task<IActionResult> InscribirMateria(int estudianteId, int materiaId)
        {
            var resultado = await _registroService.InscribirEstudianteAMateria(estudianteId, materiaId);
            if (resultado.Success)
                TempData["Mensaje"] = resultado.Message;
            else
                TempData["Error"] = resultado.Message;

            return RedirectToAction("MateriasDisponibles", new { id = estudianteId });
        }

        // GET: Registros/PorEstudianteDetalle
        [HttpPost("PorEstudianteDetalle")]
        public async Task<IActionResult> PorEstudianteDetalle()
        {
            // Obtener el ID del estudiante actual desde la sesión de usuario
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Si no hay usuario autenticado o no es un estudiante, redirigir al login
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Buscar los registros (inscripciones) del estudiante actual
            var inscripciones = await _context.Registros
                .Include(i => i.Materia)
                .Where(i => i.EstudianteId == userId)
                .ToListAsync();

            return View(inscripciones);
        }

        // POST: Registros/CancelarInscripcion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarInscripcion(int registroId)
        {
            var registro = await _context.Registros.FindAsync(registroId);
            if (registro == null) return NotFound();

            var resultado = await _registroService.EliminarInscripcion(registroId);
            if (resultado.Success) TempData["Mensaje"] = resultado.Message;
            else TempData["Error"] = resultado.Message;

            return RedirectToAction("PorEstudiante", new { id = registro.EstudianteId });
        }
    }
}