using Microsoft.AspNetCore.Mvc;
using Los_Patitos.Business;
using Los_Patitos.Models;

namespace Los_Patitos.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        // LISTA USUARIOS POR COMERCIO
        // GET: /Usuario/Index?idComercio=5
        public async Task<IActionResult> Index(int idComercio)
        {
            var usuarios = await _usuarioService.ListarPorComercioAsync(idComercio);

            // Esto sirve para recordar el comercio actual en Crear y Editar
            ViewBag.IdComercio = idComercio;

            return View(usuarios);
        }


        // GET: CREAR USUARIO
        public IActionResult Crear(int idComercio)
        {
            return View(new UsuarioModel
            {
                IdComercio = idComercio
            });
        }


        // POST: CREAR USUARIO

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([Bind(
            "IdComercio,Nombres,PrimerApellido,SegundoApellido,Identificacion,CorreoElectronico"
        )] UsuarioModel modelo)
        {
            ModelState.Remove(nameof(UsuarioModel.FechaDeRegistro));
            ModelState.Remove(nameof(UsuarioModel.FechaDeModificacion));
            ModelState.Remove(nameof(UsuarioModel.Estado));
            ModelState.Remove(nameof(UsuarioModel.Comercio));

            if (!ModelState.IsValid)
                return View(modelo);

            var (ok, error, idUsuario) = await _usuarioService.RegistrarAsync(modelo);

            if (!ok)
            {
                ViewBag.Error = error;
                return View(modelo);
            }

            TempData["Ok"] = $"Usuario registrado con el Id #{idUsuario}.";

            return RedirectToAction(nameof(Index), new { idComercio = modelo.IdComercio });
        }


        // GET: EDITAR USUARIO
        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _usuarioService.ObtenerPorIdAsync(id);
            if (usuario is null)
                return NotFound();

            return View(usuario);
        }


        // POST: EDITAR USUARIO

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar([Bind(
            "IdUsuario,IdComercio,Nombres,PrimerApellido,SegundoApellido,Identificacion,CorreoElectronico,Estado"
        )] UsuarioModel modelo)
        {
            ModelState.Remove(nameof(UsuarioModel.FechaDeRegistro));
            ModelState.Remove(nameof(UsuarioModel.FechaDeModificacion));
            ModelState.Remove(nameof(UsuarioModel.Comercio));

            if (!ModelState.IsValid)
                return View(modelo);

            var (ok, error) = await _usuarioService.EditarAsync(modelo);

            if (!ok)
            {
                ViewBag.Error = error;
                return View(modelo);
            }

            TempData["Ok"] = $"Usuario #{modelo.IdUsuario} modificado correctamente.";

            // Volver al listado de usuarios del comercio
            return RedirectToAction(nameof(Index), new { idComercio = modelo.IdComercio });
        }
    }
}