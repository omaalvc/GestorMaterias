using System.ComponentModel.DataAnnotations;

namespace GestorMaterias.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder los 50 caracteres")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato de email es inválido")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Nombre Completo")]
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Display(Name = "Es Administrador")]
        public bool EsAdministrador { get; set; } = false;

        // Relación con Estudiante (si el usuario es un estudiante)
        public int? EstudianteId { get; set; }
        public Estudiante? Estudiante { get; set; }

        // Fecha de registro
        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}