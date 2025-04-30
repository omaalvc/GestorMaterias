using GestorMaterias.Models;
using GestorMaterias.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorMaterias.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class MateriasController : ControllerBase
    {
        private readonly IMateriaService _materiaService;
        private readonly IProfesorService _profesorService;

        //    public MateriasController(IMateriaService materiaService)
        //     {
        //         _materiaService = materiaService;
        //         //_profesorService = profesorService;
        //     }

        public MateriasController(IMateriaService materiaService, IProfesorService profesorService)
        {
            _materiaService = materiaService;
            _profesorService = profesorService;
        }

        // GET: /Materias
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var materias = await _materiaService.ObtenerMateriasParaAPI();
                return Ok(materias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener materias: {ex.Message}" });
            }
        }

        // GET: /Materias/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var materia = await _materiaService.ObtenerMateriaPorIdParaAPI(id);
                if (materia == null)
                {
                    return NotFound(new { message = "Materia no encontrada" });
                }
                return Ok(materia);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener los detalles de la materia: {ex.Message}" });
            }
        }

        // POST: /Materias
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create([FromBody] Materia materia)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var resultado = await _materiaService.CrearMateria(materia);
                
                if (resultado.Success)
                {
                    return CreatedAtAction(nameof(Details), new { id = materia.Id }, materia);
                }
                
                return BadRequest(new { message = resultado.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al crear la materia: {ex.Message}" });
            }
        }

        // PUT: /Materias/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id, [FromBody] Materia materia)
        {
            if (id != materia.Id)
            {
                return BadRequest(new { message = "El ID de la materia no coincide" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var resultado = await _materiaService.ActualizarMateria(materia);
                
                if (resultado.Success)
                {
                    return NoContent();
                }
                
                return BadRequest(new { message = resultado.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al actualizar la materia: {ex.Message}" });
            }
        }

        // DELETE: /Materias/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var resultado = await _materiaService.EliminarMateria(id);
                
                if (resultado.Success)
                {
                    return NoContent();
                }
                
                return BadRequest(new { message = resultado.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al eliminar la materia: {ex.Message}" });
            }
        }

        // GET: /Materias/Profesores
        [HttpGet("Profesores")]
        public async Task<IActionResult> GetProfesores()
        {
            try
            {
                var profesores = await _profesorService.ObtenerTodosProfesores();
                return Ok(profesores.Select(p => new 
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    //Apellido = p.Apellido,
                    NombreCompleto = $"{p.Nombre}"
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener profesores: {ex.Message}" });
            }
        }

        // GET: /Materias/Profesor/5
        [HttpGet("Profesor/{profesorId}")]
        public async Task<IActionResult> GetMateriasPorProfesor(int profesorId)
        {
            try
            {
                var materias = await _materiaService.ObtenerMateriasPorProfesor(profesorId);
                return Ok(materias.Select(m => new
                {
                    Id = m.Id,
                    Nombre = m.Nombre,
                    Descripcion = m.Descripcion,
                    Creditos = m.Creditos,
                    CantidadEstudiantes = m.Registros?.Count(r => r.Estado == "Activo") ?? 0
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener materias del profesor: {ex.Message}" });
            }
        }
    }


}