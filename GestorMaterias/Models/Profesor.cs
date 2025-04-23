using System.ComponentModel.DataAnnotations;

namespace GestorMaterias.Models
{
    public class Profesor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; } = string.Empty;

        // Cada profesor puede impartir hasta 2 materias
        public List<Materia> Materias { get; set; } = new List<Materia>();

        // Validar si el profesor puede dar más materias
        public bool PuedeImpartirMasMaterias()
        {
            return Materias.Count < 2;
        }
    }
}