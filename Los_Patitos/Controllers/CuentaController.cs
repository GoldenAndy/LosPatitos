using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Los_Patitos.Models;
using Los_Patitos.Business;

namespace Los_Patitos.Controllers
{
    public class CuentaController : Controller
    {
        private readonly UserManager<UsuarioIdentity> _userManager;
        private readonly SignInManager<UsuarioIdentity> _signInManager;
        private readonly RoleManager<RolIdentity> _roleManager;
        private readonly IUsuarioService _usuarioService;

        public CuentaController(
            UserManager<UsuarioIdentity> userManager,
            SignInManager<UsuarioIdentity> signInManager,
            RoleManager<RolIdentity> roleManager,
            IUsuarioService usuarioService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _usuarioService = usuarioService;
        }

        // =======================
        //  LOGIN
        // =======================
        [AllowAnonymous]
        [HttpGet]
        public IActionResult IniciarSesion(string? returnUrl = null)
        {
            return View(new InicioSesionViewModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IniciarSesion(InicioSesionViewModel modelo)
        {
            if (!ModelState.IsValid) return View(modelo);

            // Puedes ir directo con el email como UserName (porque en Registro lo igualamos)
            var result = await _signInManager.PasswordSignInAsync(
                userName: modelo.CorreoElectronico,
                password: modelo.Contrasena,
                isPersistent: modelo.Recordarme,
                lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                return View(modelo);
            }

            if (!string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl))
                return Redirect(modelo.ReturnUrl);

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
            return View(new RegistroViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistroViewModel modelo)
        {
            if (!ModelState.IsValid) return View(modelo);

            // Validación de rol permitido
            if (!(modelo.Rol == "Administrador" || modelo.Rol == "Cajero"))
            {
                ModelState.AddModelError("Rol", "Rol inválido.");
                return View(modelo);
            }

            // Regla del profesor: si es Cajero, debe existir previamente en Usuario_G4
            UsuarioModel? usuarioNegocio = null;
            if (modelo.Rol == "Cajero")
            {
                usuarioNegocio = await _usuarioService.ObtenerPorCorreoAsync(modelo.CorreoElectronico);
                if (usuarioNegocio is null)
                {
                    ModelState.AddModelError(string.Empty,
                        "Para registrar un Cajero primero debe existir en el sistema (Usuario_G4) con ese correo.");
                    return View(modelo);
                }
            }

            // Crear usuario Identity
            var user = new UsuarioIdentity
            {
                UserName = modelo.CorreoElectronico,
                Email = modelo.CorreoElectronico
            };

            var create = await _userManager.CreateAsync(user, modelo.Contrasena);
            if (!create.Succeeded)
            {
                foreach (var e in create.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(modelo);
            }

            // Asegura que el rol exista y asigna
            if (!await _roleManager.RoleExistsAsync(modelo.Rol))
                await _roleManager.CreateAsync(new RolIdentity { Name = modelo.Rol });

            await _userManager.AddToRoleAsync(user, modelo.Rol);

            // Si es Cajero: enlaza IdNetUser en Usuario_G4
            if (modelo.Rol == "Cajero" && usuarioNegocio != null)
            {
                await _usuarioService.ActualizarIdNetUserAsync(usuarioNegocio.IdUsuario, user.Id);
            }

            TempData["Ok"] = "Usuario registrado correctamente. Ya puede iniciar sesión.";
            return RedirectToAction(nameof(IniciarSesion));
        }

        // =======================
        //  ACCESO DENEGADO
        // =======================
        [HttpGet]
        public IActionResult AccesoDenegado() => View();
    }
}
