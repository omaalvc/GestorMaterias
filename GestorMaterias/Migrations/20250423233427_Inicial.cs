using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GestorMaterias.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Estudiantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estudiantes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Profesores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profesores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EsAdministrador = table.Column<bool>(type: "bit", nullable: false),
                    EstudianteId = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Estudiantes_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "Estudiantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Materias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Creditos = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    ProfesorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Materias_Profesores_ProfesorId",
                        column: x => x.ProfesorId,
                        principalTable: "Profesores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Registros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaInscripcion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstudianteId = table.Column<int>(type: "int", nullable: false),
                    MateriaId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Registros_Estudiantes_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "Estudiantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Registros_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Profesores",
                columns: new[] { "Id", "Email", "Nombre" },
                values: new object[,]
                {
                    { 1, "juan.perez@universidad.edu", "Juan Pérez" },
                    { 2, "maria.gonzalez@universidad.edu", "María González" },
                    { 3, "carlos.rodriguez@universidad.edu", "Carlos Rodríguez" },
                    { 4, "ana.martinez@universidad.edu", "Ana Martínez" },
                    { 5, "luis.sanchez@universidad.edu", "Luis Sánchez" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Email", "EsAdministrador", "EstudianteId", "FechaRegistro", "NombreCompleto", "Password", "Username" },
                values: new object[] { 1, "admin@gestormaterias.com", true, null, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Administrador del Sistema", "admin123", "admin" });

            migrationBuilder.InsertData(
                table: "Materias",
                columns: new[] { "Id", "Creditos", "Descripcion", "Nombre", "ProfesorId" },
                values: new object[,]
                {
                    { 1, 3, "Fundamentos de cálculo y álgebra", "Matemáticas I", 1 },
                    { 2, 3, "Principios fundamentales de la física", "Física Básica", 1 },
                    { 3, 3, "Introducción a la programación", "Programación I", 2 },
                    { 4, 3, "Diseño y administración de bases de datos", "Base de Datos", 2 },
                    { 5, 3, "Inglés aplicado a contextos técnicos", "Inglés Técnico", 3 },
                    { 6, 3, "Habilidades de comunicación profesional", "Comunicación Efectiva", 3 },
                    { 7, 3, "Fundamentos de sistemas operativos", "Sistemas Operativos", 4 },
                    { 8, 3, "Principios de redes y comunicaciones", "Redes de Computadores", 4 },
                    { 9, 3, "Metodologías de desarrollo de software", "Ingeniería de Software", 5 },
                    { 10, 3, "Fundamentos de IA y machine learning", "Inteligencia Artificial", 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Materias_ProfesorId",
                table: "Materias",
                column: "ProfesorId");

            migrationBuilder.CreateIndex(
                name: "IX_Registros_EstudianteId_MateriaId",
                table: "Registros",
                columns: new[] { "EstudianteId", "MateriaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registros_MateriaId",
                table: "Registros",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EstudianteId",
                table: "Usuarios",
                column: "EstudianteId",
                unique: true,
                filter: "[EstudianteId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Username",
                table: "Usuarios",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Registros");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Materias");

            migrationBuilder.DropTable(
                name: "Estudiantes");

            migrationBuilder.DropTable(
                name: "Profesores");
        }
    }
}
