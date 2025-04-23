using System.ComponentModel.DataAnnotations;

namespace GestorMaterias.Models
{
    public class Materia
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la materia es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        // Según los requerimientos, cada materia equivale a 3 créditos
        [Display(Name = "Créditos")]
        public int Creditos { get; set; } = 3; 

        // Relación con el profesor que imparte la materia
        [Required(ErrorMessage = "La materia debe tener un profesor asignado")]
        public Profesor Profesor { get; set; } = null!;
        public int ProfesorId { get; set; }

        // Relación con las inscripciones (registros)
        public List<Registro> Registros { get; set; } = new List<Registro>();

        // Método para obtener lista de estudiantes inscritos
        public List<Estudiante> ObtenerEstudiantesInscritos()
        {
            return Registros.Select(r => r.Estudiante).ToList();
        }
    }
}