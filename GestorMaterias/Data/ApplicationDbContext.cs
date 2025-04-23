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
        public DbSet<Usuario> Usuarios { get; set; }

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
                
            // Configuración de Usuario
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Username)
                .IsUnique();
                
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();
                
            // Relación Usuario - Estudiante (opcional)
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Estudiante)
                .WithOne() // Un estudiante puede tener un solo usuario
                .HasForeignKey<Usuario>(u => u.EstudianteId)
                .IsRequired(false) // La relación es opcional
                .OnDelete(DeleteBehavior.SetNull);
                
            // Datos semilla para prueba (10 materias y 5 profesores)
            SeedData(modelBuilder);
        }
        
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Crear profesores de prueba
            var profesores = new List<Profesor>
            {
                new Profesor { Id = 1, Nombre = "Juan Pérez", Email = "juan.perez@universidad.edu" },
                new Profesor { Id = 2, Nombre = "María González", Email = "maria.gonzalez@universidad.edu" },
                new Profesor { Id = 3, Nombre = "Carlos Rodríguez", Email = "carlos.rodriguez@universidad.edu" },
                new Profesor { Id = 4, Nombre = "Ana Martínez", Email = "ana.martinez@universidad.edu" },
                new Profesor { Id = 5, Nombre = "Luis Sánchez", Email = "luis.sanchez@universidad.edu" }
            };
            
            modelBuilder.Entity<Profesor>().HasData(profesores);
            
            // Crear 10 materias (2 por cada profesor)
            var materias = new List<Materia>
            {
                new Materia { Id = 1, Nombre = "Matemáticas I", Descripcion = "Fundamentos de cálculo y álgebra", ProfesorId = 1, Creditos = 3 },
                new Materia { Id = 2, Nombre = "Física Básica", Descripcion = "Principios fundamentales de la física", ProfesorId = 1, Creditos = 3 },
                
                new Materia { Id = 3, Nombre = "Programación I", Descripcion = "Introducción a la programación", ProfesorId = 2, Creditos = 3 },
                new Materia { Id = 4, Nombre = "Base de Datos", Descripcion = "Diseño y administración de bases de datos", ProfesorId = 2, Creditos = 3 },
                
                new Materia { Id = 5, Nombre = "Inglés Técnico", Descripcion = "Inglés aplicado a contextos técnicos", ProfesorId = 3, Creditos = 3 },
                new Materia { Id = 6, Nombre = "Comunicación Efectiva", Descripcion = "Habilidades de comunicación profesional", ProfesorId = 3, Creditos = 3 },
                
                new Materia { Id = 7, Nombre = "Sistemas Operativos", Descripcion = "Fundamentos de sistemas operativos", ProfesorId = 4, Creditos = 3 },
                new Materia { Id = 8, Nombre = "Redes de Computadores", Descripcion = "Principios de redes y comunicaciones", ProfesorId = 4, Creditos = 3 },
                
                new Materia { Id = 9, Nombre = "Ingeniería de Software", Descripcion = "Metodologías de desarrollo de software", ProfesorId = 5, Creditos = 3 },
                new Materia { Id = 10, Nombre = "Inteligencia Artificial", Descripcion = "Fundamentos de IA y machine learning", ProfesorId = 5, Creditos = 3 }
            };
            
            modelBuilder.Entity<Materia>().HasData(materias);
            
            // Usuario administrador por defecto con fecha fija
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario { 
                    Id = 1, 
                    Username = "admin", 
                    Password = "admin123", 
                    Email = "admin@gestormaterias.com",
                    NombreCompleto = "Administrador del Sistema",
                    EsAdministrador = true,
                    FechaRegistro = new DateTime(2023, 1, 1) // Fecha estática
                }
            );
        }
    }
}