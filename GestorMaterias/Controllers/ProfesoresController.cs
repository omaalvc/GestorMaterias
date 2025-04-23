using GestorMaterias.Data;
using GestorMaterias.Models;
using GestorMaterias.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestorMaterias.Controllers
{
    public class ProfesoresController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMateriaService _materiaService;

        public ProfesoresController(ApplicationDbContext context, IMateriaService materiaService)
        {
            _context = context;
            _materiaService = materiaService;
        }

        // GET: Profesores
        public async Task<IActionResult> Index()
        {
            var profesores = await _context.Profesores
                .Include(p => p.Materias)
                .ToListAsync();
            return View(profesores);
        }

        // GET: Profesores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profesor = await _context.Profesores
                .Include(p => p.Materias)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (profesor == null)
            {
                return NotFound();
            }

            // Obtener todas las materias que imparte el profesor
            ViewBag.Materias = await _materiaService.ObtenerMateriasPorProfesor(profesor.Id);

            return View(profesor);
        }

        // GET: Profesores/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Profesores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Email")] Profesor profesor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(profesor);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Profesor creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(profesor);
        }

        // GET: Profesores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profesor = await _context.Profesores.FindAsync(id);
            if (profesor == null)
            {
                return NotFound();
            }
            return View(profesor);
        }

        // POST: Profesores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Email")] Profesor profesor)
        {
            if (id != profesor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(profesor);
                    await _context.SaveChangesAsync();
                    TempData["Mensaje"] = "Profesor actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfesorExists(profesor.Id))
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
            return View(profesor);
        }

        // GET: Profesores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profesor = await _context.Profesores
                .Include(p => p.Materias)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (profesor == null)
            {
                return NotFound();
            }

            // Validar si tiene materias asignadas
            if (profesor.Materias.Any())
            {
                ViewBag.Error = "No se puede eliminar el profesor porque tiene materias asignadas.";
            }

            return View(profesor);
        }

        // POST: Profesores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var profesor = await _context.Profesores
                .Include(p => p.Materias)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (profesor == null)
            {
                return NotFound();
            }

            // Verificar si tiene materias asignadas
            if (profesor.Materias.Any())
            {
                TempData["Error"] = "No se puede eliminar el profesor porque tiene materias asignadas.";
                return RedirectToAction(nameof(Index));
            }

            _context.Profesores.Remove(profesor);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Profesor eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private bool ProfesorExists(int id)
        {
            return _context.Profesores.Any(e => e.Id == id);
        }
    }
}