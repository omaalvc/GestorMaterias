using GestorMaterias.Data;
using GestorMaterias.Models;
using GestorMaterias.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestorMaterias.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class EstudiantesController : ControllerBase
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

        // GET: api/Estudiantes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetEstudiantes()
        {
            return await _context.Estudiantes
                .Select(e => new
                {
                    id = e.Id,
                    nombre = e.Nombre,
                    email = e.Email
                })
                .ToListAsync();
        }

        // GET: api/Estudiantes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetEstudiante(int id)
        {
            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(m => m.Id == id);

            if (estudiante == null)
            {
                return NotFound();
            }

            // Obtener las materias inscritas por el estudiante
            var registros = await _registroService.ObtenerRegistrosPorEstudiante(estudiante.Id);
            var materias = registros.Select(r => new
            {
                id = r.Materia.Id,
                nombre = r.Materia.Nombre,
                descripcion = r.Materia.Descripcion,
                creditos = r.Materia.Creditos
            }).ToList();

            return new
            {
                estudiante = new
                {
                    id = estudiante.Id,
                    nombre = estudiante.Nombre,
                    email = estudiante.Email
                },
                materias = materias
            };
        }

        // POST: api/Estudiantes
        [HttpPost]
        public async Task<ActionResult<object>> CreateEstudiante([FromBody] Estudiante estudiante)
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
                
                return new
                {
                    id = estudiante.Id,
                    nombre = estudiante.Nombre,
                    email = estudiante.Email
                };
            }
            return BadRequest(ModelState);
        }

        // PUT: api/Estudiantes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEstudiante(int id, [FromBody] Estudiante estudiante)
        {
            if (id != estudiante.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the original student to compare changes
                    var originalEstudiante = await _context.Estudiantes
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
                    
                    if (originalEstudiante == null)
                    {
                        return NotFound();
                    }
                    
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
                            usuario.NombreCompleto = estudiante.Nombre;
                        }
                        
                        // Update email if changed
                        usuario.Email = estudiante.Email;
                        
                        await _context.SaveChangesAsync();
                    }

                    return NoContent();
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
            }
            return BadRequest(ModelState);
        }

        // DELETE: api/Estudiantes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEstudiante(int id)
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
                return BadRequest(new { message = "No se puede eliminar el estudiante porque está matriculado en una o más materias." });
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
            return NoContent();
        }

        // POST: api/Estudiantes/5/materias/2
        [HttpPost("{estudianteId}/materias/{materiaId}")]
        public async Task<IActionResult> MatricularEstudiante(int estudianteId, int materiaId)
        {
            var estudiante = await _context.Estudiantes.FindAsync(estudianteId);
            var materia = await _context.Materias.FindAsync(materiaId);
            
            if (estudiante == null || materia == null)
            {
                return NotFound();
            }
            
            // Verificar si ya está matriculado
            var registroExistente = await _context.Registros
                .FirstOrDefaultAsync(r => r.EstudianteId == estudianteId && r.MateriaId == materiaId);
                
            if (registroExistente != null)
            {
                return BadRequest(new { message = "El estudiante ya está matriculado en esta materia." });
            }
            
            var registro = new Registro
            {
                EstudianteId = estudianteId,
                MateriaId = materiaId,
                //FechaRegistro = DateTime.Now
            };
            
            _context.Registros.Add(registro);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        // DELETE: api/Estudiantes/5/materias/2
        [HttpDelete("{estudianteId}/materias/{materiaId}")]
        public async Task<IActionResult> CancelarMatricula(int estudianteId, int materiaId)
        {
            var registro = await _context.Registros
                .FirstOrDefaultAsync(r => r.EstudianteId == estudianteId && r.MateriaId == materiaId);
                
            if (registro == null)
            {
                return NotFound();
            }
            
            _context.Registros.Remove(registro);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        // GET: api/Estudiantes/5/companeros/2
        [HttpGet("{estudianteId}/companeros/{materiaId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetCompaneros(int estudianteId, int materiaId)
        {
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
                .Where(r => r.EstudianteId != estudianteId)
                .Select(r => new
                {
                    id = r.Estudiante.Id,
                    nombre = r.Estudiante.Nombre,
                    email = r.Estudiante.Email
                })
                .ToList();

            return companeros;
        }

        private bool EstudianteExists(int id)
        {
            return _context.Estudiantes.Any(e => e.Id == id);
        }
    }
}