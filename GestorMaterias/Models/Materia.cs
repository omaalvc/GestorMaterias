using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestorMaterias.Models
{
    public class Materia
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la materia es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La descripción de la materia es requerida")]
        public string Descripcion { get; set; }

        [Display(Name = "Créditos")]
        public int Creditos { get; set; } = 3; 

        // Eliminar validación Required para resolver problema
        [Display(Name = "Profesor")]
        public int ProfesorId { get; set; }

        public Profesor Profesor { get; set; }

        public List<Registro> Registros { get; set; } = new List<Registro>();

        [NotMapped]
        public List<Estudiante> Estudiantes { get; set; } = new List<Estudiante>();

        public List<Estudiante> ObtenerEstudiantesInscritos()
        {
            return Registros.Select(r => r.Estudiante).ToList();
        }
    }
}