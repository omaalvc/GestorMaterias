using GestorMaterias.Models;
using GestorMaterias.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestorMaterias.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegistrosController : ControllerBase
    {
        private readonly IRegistroService _registroService;

        public RegistrosController(IRegistroService registroService)
        {
            _registroService = registroService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Registro>>> GetRegistros()
        {
            var registros = await _registroService.ObtenerTodosLosRegistros();
            return Ok(registros);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Registro>> GetRegistro(int id)
        {
            var registros = await _registroService.ObtenerTodosLosRegistros();
            var registro = registros.FirstOrDefault(r => r.Id == id);
                
            if (registro == null)
            {
                return NotFound(new { message = "Registro no encontrado" });
            }

            return Ok(registro);
        }

        [HttpGet("PorEstudiante/{estudianteId}")]
        public async Task<ActionResult<IEnumerable<Registro>>> GetRegistrosPorEstudiante(int estudianteId)
        {
            var registros = await _registroService.ObtenerRegistrosPorEstudiante(estudianteId);
            return Ok(registros);
        }

        [HttpGet("MateriasDisponibles/{estudianteId}")]
        public async Task<ActionResult<IEnumerable<Materia>>> GetMateriasDisponibles(int estudianteId)
        {
            var materias = await _registroService.GetMateriasDisponibles(estudianteId);
            return Ok(materias);
        }

        [HttpGet("MateriasEstudiante/{estudianteId}")]
        public async Task<ActionResult<IEnumerable<Materia>>> GetMateriasEstudiante(int estudianteId)
        {
            var materias = await _registroService.GetMateriasEstudiante(estudianteId);
            return Ok(materias);
        }

        [HttpPost("InscribirMateria")]
        public async Task<IActionResult> InscribirMateria([FromQuery] int estudianteId, [FromQuery] int materiaId)
        {
            try
            {
                var result = await _registroService.MatricularEstudiante(estudianteId, materiaId);
                if (result)
                {
                    return Ok(new { success = true, message = "Inscripción realizada con éxito" });
                }
                
                // Verificar razones específicas del fallo
                var estudiante = await _registroService.GetMateriasEstudiante(estudianteId);
                if (estudiante.Count >= 3)
                {
                    return BadRequest(new { success = false, message = "El estudiante ya tiene el máximo de materias permitidas (3)" });
                }
                
                if (estudiante.Any(m => m.Id == materiaId))
                {
                    return BadRequest(new { success = false, message = "El estudiante ya está inscrito en esta materia" });
                }
                
                return BadRequest(new { success = false, message = "No se pudo realizar la inscripción" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("CancelarMatricula")]
        public async Task<IActionResult> CancelarMatricula([FromQuery] int estudianteId, [FromQuery] int materiaId)
        {
            var result = await _registroService.CancelarMatricula(estudianteId, materiaId);
            if (result)
            {
                return Ok(new { message = "Matrícula cancelada con éxito" });
            }
            return BadRequest(new { message = "No se pudo cancelar la matrícula" });
        }

        [HttpPost("CancelarInscripcion")]
        public async Task<IActionResult> CancelarInscripcion(int registroId)
        {
            var result = await _registroService.CancelarInscripcion(registroId);
            if (result.Success)
            {
                return Ok(new { message = result.Message });
            }
            return BadRequest(new { message = result.Message });
        }
    }
}