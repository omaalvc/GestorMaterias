using GestorMaterias.Models;
using GestorMaterias.Models.ViewModels;
using GestorMaterias.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestorMaterias.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUsuarioService _usuarioService;

        public AccountController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var resultado = await _usuarioService.ValidarCredenciales(model.Username, model.Password);
                    if (resultado.success)
                    {
                        // Crear los claims para el usuario
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, resultado.usuario.Username),
                            new Claim(ClaimTypes.Email, resultado.usuario.Email),
                            new Claim("FullName", resultado.usuario.NombreCompleto),
                            new Claim(ClaimTypes.Role, resultado.usuario.EsAdministrador ? "Administrador" : "Estudiante")
                        };

                        if (resultado.usuario.EstudianteId.HasValue)
                        {
                            claims.Add(new Claim("EstudianteId", resultado.usuario.EstudianteId.Value.ToString()));
                        }

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(3)
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            principal,
                            authProperties);

                        if (string.IsNullOrEmpty(returnUrl))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        return Redirect(returnUrl);
                    }
                    
                    ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al iniciar sesión: " + ex.Message);
                return View(model);
            }
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistroUsuarioViewModel model)
        {
            if (ModelState.IsValid)
            {
            // Crear un objeto Estudiante para asociarlo con el usuario
            var estudiante = new Estudiante
            {
                Nombre = model.NombreCompleto,
                Email = model.Email,
                //FechaRegistro = DateTime.Now,
                //Activo = true
            };
            
            // Crear el usuario y asociarlo con el estudiante
            var usuario = new Usuario
            {
                Username = model.Username,
                Email = model.Email,
                NombreCompleto = model.NombreCompleto,
                FechaRegistro = DateTime.Now,
                // Se asignará el EstudianteId después de crear el estudiante
            };

            var resultado = await _usuarioService.RegistrarUsuarioYEstudiante(usuario, estudiante, model.Password);
            
            if (resultado.success)
            {
                // Iniciar sesión automáticamente después del registro
                await IniciarSesion(resultado.usuario, false);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.message);
            }
            }
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async Task IniciarSesion(Usuario usuario, bool isPersistent)
        {
            // Crear claims de identidad
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Username),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim("FullName", usuario.NombreCompleto),
                new Claim(ClaimTypes.Role, usuario.EsAdministrador ? "Administrador" : "Estudiante")
            };

            if (usuario.EstudianteId.HasValue)
            {
                claims.Add(new Claim("EstudianteId", usuario.EstudianteId.Value.ToString()));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}