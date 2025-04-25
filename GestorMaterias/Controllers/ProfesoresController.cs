using GestorMaterias.Data;
using GestorMaterias.Models;
using GestorMaterias.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace GestorMaterias.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme + "," + JwtBearerDefaults.AuthenticationScheme)]
    public class ProfesoresController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMateriaService _materiaService;

        public ProfesoresController(ApplicationDbContext context, IMateriaService materiaService)
        {
            _context = context;
            _materiaService = materiaService;
        }

        // GET: /Profesores
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var profesores = await _context.Profesores
                .Include(p => p.Materias)
                .ToListAsync();
                
            // Si es una solicitud AJAX o API, devolver JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                return Ok(profesores);
            }
            
            // De lo contrario, devolver una vista
            return View(profesores);
        }

        // GET: /Profesores/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var profesor = await _context.Profesores
                .Include(p => p.Materias)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (profesor == null)
            {
                return NotFound();
            }

            var materias = await _materiaService.ObtenerMateriasPorProfesor(profesor.Id);
            
            // Si es una solicitud AJAX o API, devolver JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                return Ok(new { profesor, materias });
            }
            
            // De lo contrario, pasar datos a la vista
            ViewBag.Materias = materias;
            return View(profesor);
        }

        // GET: /Profesores/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Profesores/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<Profesor>> Create(Profesor profesor)
        {
            if (!ModelState.IsValid)
                return View(profesor);

            _context.Profesores.Add(profesor);
            await _context.SaveChangesAsync();

            // Si es una solicitud AJAX o API, devolver JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                return CreatedAtAction(nameof(Details), new { id = profesor.Id }, profesor);
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: /Profesores/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var profesor = await _context.Profesores.FindAsync(id);
            if (profesor == null)
            {
                return NotFound();
            }
            return View(profesor);
        }

        // POST: /Profesores/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> Edit(int id, [FromForm] Profesor profesor)
        {
            if (id != profesor.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(profesor);

            try
            {
                var profesorExistente = await _context.Profesores.FindAsync(id);
                if (profesorExistente == null)
                {
                    return NotFound();
                }

                // Actualizar propiedades
                profesorExistente.Nombre = profesor.Nombre;
                profesorExistente.Email = profesor.Email;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfesorExists(id))
                    return NotFound();
                throw;
            }

            // Si es una solicitud AJAX o API, devolver resultado 200
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                return Ok(profesor);
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: /Profesores/Delete/5
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var profesor = await _context.Profesores
                .Include(p => p.Materias)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profesor == null)
            {
                return NotFound();
            }

            return View(profesor);
        }

        // POST: /Profesores/Delete/5
        [HttpPost("Delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var profesor = await _context.Profesores
                .Include(p => p.Materias)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profesor == null)
                return NotFound();

            if (profesor.Materias.Any())
            {
                ModelState.AddModelError(string.Empty, "No se puede eliminar el profesor porque tiene materias asignadas.");
                return View(profesor);
            }

            _context.Profesores.Remove(profesor);
            await _context.SaveChangesAsync();

            // Si es una solicitud AJAX o API, devolver NoContent
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                return NoContent();
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool ProfesorExists(int id)
        {
            return _context.Profesores.Any(e => e.Id == id);
        }

        // Este endpoint es solo para API
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Consumes("application/json")]
        public async Task<IActionResult> UpdateProfesor(int id, [FromBody] Profesor profesor)
        {
            if (id != profesor.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Obtener el profesor actual para actualizarlo
                var profesorExistente = await _context.Profesores.FindAsync(id);
                
                if (profesorExistente == null)
                    return NotFound();
                
                // Actualizar propiedades
                profesorExistente.Nombre = profesor.Nombre;
                profesorExistente.Email = profesor.Email;
                
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfesorExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // Este endpoint es solo para API
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteProfesor(int id)
        {
            var profesor = await _context.Profesores
                .Include(p => p.Materias)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profesor == null)
                return NotFound();

            if (profesor.Materias.Any())
                return BadRequest("No se puede eliminar el profesor porque tiene materias asignadas.");

            _context.Profesores.Remove(profesor);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}