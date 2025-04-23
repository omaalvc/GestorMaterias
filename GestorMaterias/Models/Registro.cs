using System.ComponentModel.DataAnnotations;

namespace GestorMaterias.Models
{
    public class Registro
    {
        [Key]
        public int Id { get; set; }

        // Fecha de inscripción
        [Display(Name = "Fecha de Inscripción")]
        public DateTime FechaInscripcion { get; set; } = DateTime.Now;

        // Relación con el estudiante
        [Required]
        public Estudiante Estudiante { get; set; } = null!;
        public int EstudianteId { get; set; }

        // Relación con la materia
        [Required]
        public Materia Materia { get; set; } = null!;
        public int MateriaId { get; set; }

        // Estado de la inscripción (Activo, Cancelado, etc.)
        public string Estado { get; set; } = "Activo";
        
        // Método para validar si la inscripción es válida según las reglas
        public bool EsInscripcionValida()
        {
            // Validar que el estudiante no tenga más de 3 materias
            if (Estudiante.Registros.Count >= 3)
                return false;
            
            // Validar que el estudiante no tenga otra materia con el mismo profesor
            if (Estudiante.Registros.Any(r => r.Id != Id && r.Materia.Profesor.Id == Materia.Profesor.Id))
                return false;
            
            return true;
        }
    }
}