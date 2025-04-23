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
            
            ViewBag.Profesores = new SelectList(profesoresDisponibles, "Id", "Nombre");
            return View();
        }

        // POST: Materias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion,ProfesorId")] Materia materia)
        {
            if (ModelState.IsValid)
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

            // Recargar lista de profesores si hay un error
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

            if (ModelState.IsValid)
            {
                var resultado = await _materiaService.ActualizarMateria(materia);
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

            // Recargar profesores en caso de error
            var materiaExistente = await _materiaService.ObtenerMateriaPorId(id);
            var profesores = await _context.Profesores.ToListAsync();
            var profesoresDisponibles = new List<Profesor>();
            
            profesoresDisponibles.Add(materiaExistente.Profesor);
            
            foreach (var profesor in profesores)
            {
                if (profesor.Id != materiaExistente.ProfesorId && await _materiaService.ProfesorPuedeImpartirMasMateria(profesor.Id))
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

            var materia = await _materiaService.ObtenerMateriaPorId(id.Value);
            if (materia == null)
            {
                return NotFound();
            }

            return View(materia);
        }

        // POST: Materias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var resultado = await _materiaService.EliminarMateria(id);
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
    }
}