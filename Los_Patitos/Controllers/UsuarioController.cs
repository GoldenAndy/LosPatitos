using System.Threading.Tasks;
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

        // GET: Crear
        public IActionResult Crear()
        {
            return View(new UsuarioModel());
        }

        // POST: Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([Bind(
            "IdComercio,Nombres,PrimerApellido,SegundoApellido,Identificacion,CorreoElectronico"
        )] UsuarioModel modelo)
        {
            //Si el campo no es visible en el formulario no se valida
            ModelState.Remove(nameof(UsuarioModel.FechaDeRegistro));
            ModelState.Remove(nameof(UsuarioModel.FechaDeModificacion));
            ModelState.Remove(nameof(UsuarioModel.Estado));
            ModelState.Remove(nameof(UsuarioModel.IdNetUser));
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
            return RedirectToAction(nameof(Crear));
        }

        // GET: Editar
        public async Task<IActionResult> Editar(int id)
        {
            var lista = await _usuarioService.ListarPorComercioAsync(0);

            var usuario = lista.FirstOrDefault(x => x.IdUsuario == id);
            if (usuario is null)
                return NotFound();

            return View(usuario);
        }

        // POST: Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar([Bind(
            "IdUsuario,IdComercio,Nombres,PrimerApellido,SegundoApellido,Identificacion,CorreoElectronico,Estado"
        )] UsuarioModel modelo)
        {
            //Si el campo no se puede modificar no se valida
            ModelState.Remove(nameof(UsuarioModel.FechaDeRegistro));
            ModelState.Remove(nameof(UsuarioModel.FechaDeModificacion));
            ModelState.Remove(nameof(UsuarioModel.IdNetUser));
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
            return RedirectToAction("Index", "Usuario");
        }
    }
}
