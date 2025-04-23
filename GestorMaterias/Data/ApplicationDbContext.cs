using Microsoft.EntityFrameworkCore;
using GestorMaterias.Models;

namespace GestorMaterias.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<Profesor> Profesores { get; set; }
        public DbSet<Materia> Materias { get; set; }
        public DbSet<Registro> Registros { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de relación Estudiante - Registros
            modelBuilder.Entity<Estudiante>()
                .HasMany(e => e.Registros)
                .WithOne(r => r.Estudiante)
                .HasForeignKey(r => r.EstudianteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración de relación Profesor - Materias
            // Un profesor puede dictar máximo 2 materias
            modelBuilder.Entity<Profesor>()
                .HasMany(p => p.Materias)
                .WithOne(m => m.Profesor)
                .HasForeignKey(m => m.ProfesorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración de relación Materia - Registros
            modelBuilder.Entity<Materia>()
                .HasMany(m => m.Registros)
                .WithOne(r => r.Materia)
                .HasForeignKey(r => r.MateriaId)
                .OnDelete(DeleteBehavior.NoAction); // Para evitar conflicto con cascade delete

            // Validación: Cada materia tiene 3 créditos
            modelBuilder.Entity<Materia>()
                .Property(m => m.Creditos)
                .HasDefaultValue(3)
                .IsRequired();

            // Índices para mejorar el rendimiento de las consultas
            modelBuilder.Entity<Registro>()
                .HasIndex(r => new { r.EstudianteId, r.MateriaId })
                .IsUnique(); // Un estudiante solo puede inscribirse una vez a una materia
        }
    }
}