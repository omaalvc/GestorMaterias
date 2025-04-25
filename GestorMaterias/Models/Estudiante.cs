using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestorMaterias.Models
{
    public class Estudiante
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; }

        public List<Registro> Registros { get; set; } = new List<Registro>();

        [NotMapped]
        public List<Materia> Materias { get; set; } = new List<Materia>();

               // Método para validar si el estudiante puede inscribirse a una materia
        public bool PuedeInscribirseAMateria(Materia materia)
        {
            // Validar que no exceda el límite de 3 materias
            if (Registros.Count >= 3)
                return false;

            // Validar que no tenga ya la materia inscrita
            if (Registros.Any(r => r.Materia.Id == materia.Id))
                return false;

            // Validar que no tenga ya una materia con el mismo profesor
            if (Registros.Any(r => r.Materia.Profesor.Id == materia.Profesor.Id))
                return false;

            return true;
        }
    }
}