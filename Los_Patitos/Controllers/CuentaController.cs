using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Los_Patitos.Models;

namespace Los_Patitos.Controllers
{
    public class CuentaController : Controller
    {
        private readonly UserManager<UsuarioIdentity> _userManager;
        private readonly SignInManager<UsuarioIdentity> _signInManager;

        public CuentaController(
            UserManager<UsuarioIdentity> userManager,
            SignInManager<UsuarioIdentity> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // =======================
        //  LOGIN
        // =======================
        [AllowAnonymous]
        [HttpGet]
        public IActionResult IniciarSesion(string? returnUrl = null)
        {
            var modelo = new InicioSesionViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(modelo);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IniciarSesion(InicioSesionViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var usuario = await _userManager.FindByEmailAsync(modelo.CorreoElectronico);
            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                return View(modelo);
            }

            var resultado = await _signInManager.PasswordSignInAsync(
                usuario,
                modelo.Contrasena,
                modelo.Recordarme,
                lockoutOnFailure: false);

            if (!resultado.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                return View(modelo);
            }

            if (!string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl))
                return Redirect(modelo.ReturnUrl);

            // redirige al Home por defecto
            return RedirectToAction("Index", "Home");
        }

        // =======================
        //  LOGOUT
        // =======================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CerrarSesion()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(IniciarSesion));
        }

        // =======================
        //  REGISTRO
        // =======================

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Registrar()
        {
            // Solo devuelve la vista de registro
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistroViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var usuarioIdentity = new UsuarioIdentity
            {
                UserName = modelo.CorreoElectronico,
                Email = modelo.CorreoElectronico
            };

            var resultado = await _userManager.CreateAsync(usuarioIdentity, modelo.Contrasena);

            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(modelo);
            }

            // Asignar rol (Administrador / Cajero)
            await _userManager.AddToRoleAsync(usuarioIdentity, modelo.Rol);

            TempData["Ok"] = "Usuario registrado correctamente. Ahora puede iniciar sesión.";
            return RedirectToAction(nameof(IniciarSesion));
        }

        // =======================
        //  ACCESO DENEGADO
        // =======================
        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}
