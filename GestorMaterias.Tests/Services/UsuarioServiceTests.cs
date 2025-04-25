using GestorMaterias.Data;
using GestorMaterias.Models;
using GestorMaterias.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Threading.Tasks;

namespace GestorMaterias.Tests.Services
{
    public class UsuarioServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ApplicationDbContext _context;
        private readonly UsuarioService _usuarioService;

        public UsuarioServiceTests()
        {
            // Configurar la base de datos en memoria para pruebas
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(_options);
            _usuarioService = new UsuarioService(_context);
        }

        [Fact]
        public async Task RegistrarUsuario_DebeRegistrarCorrectamente()
        {
            // Arrange
            var usuario = new Usuario
            {
                Username = "testuser",
                Email = "test@example.com",
                NombreCompleto = "Usuario de Prueba",
                Password = "password123"
            };

            // Act
            var resultado = await _usuarioService.RegistrarUsuario(usuario, usuario.Password);

            // Assert
            Assert.True(resultado.success);
            Assert.Equal("Usuario registrado correctamente", resultado.message);
            Assert.NotNull(resultado.usuario);
            Assert.Equal("testuser", resultado.usuario.Username);

            // Verificar que se guardó en la base de datos
            var usuarioGuardado = await _context.Usuarios.FirstOrDefaultAsync(u => u.Username == "testuser");
            Assert.NotNull(usuarioGuardado);
        }

        [Fact]
        public async Task RegistrarUsuario_DebeRechazarUsuarioDuplicado()
        {
            // Arrange
            var usuario1 = new Usuario
            {
                Username = "testuser",
                Email = "test1@example.com",
                NombreCompleto = "Usuario de Prueba 1",
                Password = "password123"
            };

            var usuario2 = new Usuario
            {
                Username = "testuser", // Mismo username
                Email = "test2@example.com",
                NombreCompleto = "Usuario de Prueba 2",
                Password = "password456"
            };

            await _usuarioService.RegistrarUsuario(usuario1, usuario1.Password);

            // Act
            var resultado = await _usuarioService.RegistrarUsuario(usuario2, usuario2.Password);

            // Assert
            Assert.False(resultado.success);
            Assert.Equal("El nombre de usuario ya existe", resultado.message);
            Assert.Null(resultado.usuario);
        }

        [Fact]
        public async Task RegistrarUsuario_DebeRechazarEmailDuplicado()
        {
            // Arrange
            var usuario1 = new Usuario
            {
                Username = "testuser1",
                Email = "test@example.com",
                NombreCompleto = "Usuario de Prueba 1",
                Password = "password123"
            };

            var usuario2 = new Usuario
            {
                Username = "testuser2",
                Email = "test@example.com", // Mismo email
                NombreCompleto = "Usuario de Prueba 2",
                Password = "password456"
            };

            await _usuarioService.RegistrarUsuario(usuario1, usuario1.Password);

            // Act
            var resultado = await _usuarioService.RegistrarUsuario(usuario2, usuario2.Password);

            // Assert
            Assert.False(resultado.success);
            Assert.Equal("El correo electrónico ya está registrado", resultado.message);
            Assert.Null(resultado.usuario);
        }

        [Fact]
        public async Task ValidarCredenciales_DebeValidarCredencialesCorrectas()
        {
            // Arrange
            var usuario = new Usuario
            {
                Username = "testuser",
                Email = "test@example.com",
                NombreCompleto = "Usuario de Prueba",
                Password = "password123"
            };

            await _usuarioService.RegistrarUsuario(usuario, usuario.Password);

            // Act
            var resultado = await _usuarioService.ValidarCredenciales("testuser", "password123");

            // Assert
            Assert.True(resultado.success);
            Assert.Equal("Inicio de sesión exitoso", resultado.message);
            Assert.NotNull(resultado.usuario);
            Assert.Equal("testuser", resultado.usuario.Username);
        }

        [Fact]
        public async Task ValidarCredenciales_DebeRechazarPasswordIncorrecta()
        {
            // Arrange
            var usuario = new Usuario
            {
                Username = "testuser",
                Email = "test@example.com",
                NombreCompleto = "Usuario de Prueba",
                Password = "password123"
            };

            await _usuarioService.RegistrarUsuario(usuario, usuario.Password);

            // Act
            var resultado = await _usuarioService.ValidarCredenciales("testuser", "incorrectpassword");

            // Assert
            Assert.False(resultado.success);
            Assert.Equal("Contraseña incorrecta", resultado.message);
            Assert.Null(resultado.usuario);
        }

        [Fact]
        public async Task ValidarCredenciales_DebeRechazarUsuarioInexistente()
        {
            // Act
            var resultado = await _usuarioService.ValidarCredenciales("usuarioinexistente", "password123");

            // Assert
            Assert.False(resultado.success);
            Assert.Equal("Usuario no encontrado", resultado.message);
            Assert.Null(resultado.usuario);
        }

        [Fact]
        public async Task ObtenerUsuarioPorId_DebeRetornarUsuarioCorrecto()
        {
            // Arrange
            var usuario = new Usuario
            {
                Username = "testuser",
                Email = "test@example.com",
                NombreCompleto = "Usuario de Prueba",
                Password = "password123"
            };

            var resultado = await _usuarioService.RegistrarUsuario(usuario, usuario.Password);
            Assert.NotNull(resultado.usuario); // Ensure usuario is not null before accessing
            var usuarioId = resultado.usuario!.Id;

            // Act
            var usuarioObtenido = await _usuarioService.ObtenerUsuarioPorId(usuarioId);

            // Assert
            Assert.NotNull(usuarioObtenido);
            Assert.Equal("testuser", usuarioObtenido.Username);
            Assert.Equal("test@example.com", usuarioObtenido.Email);
        }

        [Fact]
        public async Task RegistrarUsuarioYEstudiante_DebeCrearAmbosRegistros()
        {
            // Arrange
            var usuario = new Usuario
            {
                Username = "student1",
                Email = "student@example.com",
                NombreCompleto = "Estudiante de Prueba"
            };

            var estudiante = new Estudiante
            {
                Nombre = "Estudiante de Prueba",
                Email = "student@example.com"
            };

            // Act
            var resultado = await _usuarioService.RegistrarUsuarioYEstudiante(usuario, estudiante, "password123");

            // Assert
            Assert.True(resultado.success);
            var usuarioResult = resultado.usuario!;
            Assert.NotNull(usuarioResult.EstudianteId);
            Assert.Equal(estudiante.Id, usuarioResult.EstudianteId!.Value);
        }

        [Fact]
        public async Task AsociarEstudianteAUsuario_DebeAsociarCorrectamente()
        {
            // Arrange
            var usuario = new Usuario
            {
                Username = "testuser",
                Email = "test@example.com",
                NombreCompleto = "Usuario de Prueba",
                Password = "password123"
            };
            _context.Usuarios.Add(usuario);

            var estudiante = new Estudiante
            {
                Nombre = "Estudiante de Prueba",
                Email = "student@example.com"
            };
            _context.Estudiantes.Add(estudiante);
            
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _usuarioService.AsociarEstudianteAUsuario(usuario.Id, estudiante.Id);

            // Assert
            Assert.True(resultado);
            
            // Verificar que se actualizó la asociación
            var usuarioActualizado = await _context.Usuarios.FindAsync(usuario.Id);
            Assert.NotNull(usuarioActualizado);
            Assert.Equal(estudiante.Id, usuarioActualizado.EstudianteId);
        }
    }
}