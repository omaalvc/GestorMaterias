using GestorMaterias.Data;
using GestorMaterias.Models;
using GestorMaterias.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestorMaterias.Controllers
{
    public class EstudiantesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRegistroService _registroService;

        private readonly IUsuarioService _usuarioService;

        public EstudiantesController(ApplicationDbContext context, IRegistroService registroService, IUsuarioService usuarioService)
        {
            _context = context;
            _registroService = registroService;
            _usuarioService = usuarioService;
        }

        // GET: Estudiantes
        public async Task<IActionResult> Index()
        {
            var estudiantes = await _context.Estudiantes
            .Include(e => e.Registros)
            .ToListAsync();
            
            // Obtener el usuario asociado a cada estudiante por correo electrónico
            foreach (var estudiante in estudiantes)
            {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == estudiante.Email);
            
            if (usuario != null)
            {
                ViewData[$"Username_{estudiante.Id}"] = usuario.Username;
            }
            }
            
            return View(estudiantes);
        }

        // GET: Estudiantes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (estudiante == null)
            {
                return NotFound();
            }

            // Obtener las materias inscritas por el estudiante
            ViewBag.Registros = await _registroService.ObtenerRegistrosPorEstudiante(estudiante.Id);

            return View(estudiante);
        }

        // GET: Estudiantes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Estudiantes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Email")] Estudiante estudiante)
        {
            if (ModelState.IsValid)
            {
            _context.Add(estudiante);
            await _context.SaveChangesAsync();
            
            string username = estudiante.Nombre.Replace(" ", "").ToLower();
            
            Usuario usuario = new Usuario
            {
                Email = estudiante.Email,
                Username = username,
                EsAdministrador = false,
                NombreCompleto = estudiante.Nombre,
            };

            await _usuarioService.RegistrarUsuario(usuario, "123456", true);
            
            return RedirectToAction(nameof(Index));
            }
            return View(estudiante);
        }

        // GET: Estudiantes/Edit/5
        public async Task<IActionResult> Edit(int? id)
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
            return View(estudiante);
        }

        // POST: Estudiantes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Email")] Estudiante estudiante)
        {
            if (id != estudiante.Id)
            {
            return NotFound();
            }

            if (ModelState.IsValid)
            {
            try
            {
                // Get the original student to compare changes
                var originalEstudiante = await _context.Estudiantes
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
                
                // Update the student
                _context.Update(estudiante);
                await _context.SaveChangesAsync();
                
                // Find and update the associated user
                var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == originalEstudiante.Email);
                
                if (usuario != null)
                {
                // Update username if name changed
                if (originalEstudiante.Nombre != estudiante.Nombre)
                {
                    usuario.Username = estudiante.Nombre.Replace(" ", "").ToLower();
                }
                
                // Update email if changed
                usuario.Email = estudiante.Email;
                
                await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstudianteExists(estudiante.Id))
                {
                return NotFound();
                }
                else
                {
                throw;
                }
            }
            return RedirectToAction(nameof(Index));
            }
            return View(estudiante);
        }

        // GET: Estudiantes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estudiante = await _context.Estudiantes
                .Include(e => e.Registros)
                .ThenInclude(r => r.Materia)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (estudiante == null)
            {
                return NotFound();
            }

            // Para la vista necesitamos obtener las materias del estudiante
            var materias = estudiante.Registros?.Select(r => r.Materia).ToList();
            estudiante.Materias = materias ?? new List<Materia>();

            // Asegúrate de que la vista esté siendo encontrada
            return View("Delete", estudiante);
        }

        // POST: Estudiantes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var estudiante = await _context.Estudiantes
                .Include(e => e.Registros)
                .FirstOrDefaultAsync(e => e.Id == id);
                
            if (estudiante == null)
            {
                return NotFound();
            }

            // Verificar si el estudiante tiene materias asociadas
            if (estudiante.Registros != null && estudiante.Registros.Any())
            {
                ViewBag.Error = "No se puede eliminar el estudiante porque está matriculado en una o más materias.";
                
                // Recargar el estudiante con sus materias para la vista
                estudiante = await _context.Estudiantes
                    .Include(e => e.Registros)
                    .ThenInclude(r => r.Materia)
                    .FirstOrDefaultAsync(e => e.Id == id);
                    
                var materias = estudiante.Registros?.Select(r => r.Materia).ToList();
                estudiante.Materias = materias ?? new List<Materia>();
                
                return View(estudiante);
            }

            // También eliminar el usuario asociado si existe
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == estudiante.Email);
                
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
            }

            _context.Estudiantes.Remove(estudiante);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EstudianteExists(int id)
        {
            return _context.Estudiantes.Any(e => e.Id == id);
        }

        // GET: Estudiantes/VerCompaneros/5 (Ver compañeros por materia)
        public async Task<IActionResult> VerCompaneros(int? id, int? materiaId)
        {
            if (id == null || materiaId == null)
            {
                return NotFound();
            }

            var materia = await _context.Materias
                .Include(m => m.Registros)
                .ThenInclude(r => r.Estudiante)
                .FirstOrDefaultAsync(m => m.Id == materiaId);

            if (materia == null)
            {
                return NotFound();
            }

            // Filtra los estudiantes que no son el estudiante actual
            var companeros = materia.Registros
                .Where(r => r.EstudianteId != id)
                .Select(r => r.Estudiante)
                .ToList();

            ViewBag.Materia = materia;
            ViewBag.EstudianteId = id;

            return View(companeros);
        }
    }
}