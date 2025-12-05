using Los_Patitos.Business;
using Los_Patitos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Los_Patitos.Controllers
{
    [Authorize(Roles = "Cajero,Administrador")]
    public class CajaController : Controller
    {
        private readonly ICajaService _cajaService;
        private readonly ISinpeService _sinpeService;
        private readonly UserManager<UsuarioIdentity> _userManager;
        private readonly IUsuarioService _usuarioService;

        public CajaController(
            ICajaService cajaService,
            ISinpeService sinpeService,
            UserManager<UsuarioIdentity> userManager,
            IUsuarioService usuarioService)
        {
            _cajaService = cajaService;
            _sinpeService = sinpeService;
            _userManager = userManager;
            _usuarioService = usuarioService;
        }

        // Helper: obtiene IdComercio del cajero logueado, o null si no está enlazado
        private async Task<int?> GetIdComercioDelUsuarioAsync()
        {
            var netUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(netUserId)) return null;
            var usuario = await _usuarioService.ObtenerPorIdNetUserAsync(netUserId);
            return usuario?.IdComercio;
        }

        // GET /Caja/Index?idComercio=#
        // - Cajero: ignora el parámetro y usa su propio IdComercio
        // - Admin: si no envía idComercio, muestra mensaje
        public async Task<IActionResult> Index(int? idComercio)
        {
            int? comercioFinal = idComercio;

            if (User.IsInRole("Cajero"))
            {
                comercioFinal = await GetIdComercioDelUsuarioAsync();
                if (comercioFinal is null) return Forbid();
            }
            else if (User.IsInRole("Administrador"))
            {
                if (comercioFinal is null)
                {
                    TempData["Error"] = "Seleccione un comercio para ver sus cajas.";
                    return RedirectToAction("Index", "Comercios"); // o una vista de selección
                }
            }

            var cajas = await _cajaService.GetByComercioAsync(comercioFinal!.Value);
            ViewBag.IdComercio = comercioFinal;
            return View(cajas);
        }

        // GET /Caja/Crear?idComercio=#
        public async Task<IActionResult> Crear(int? idComercio)
        {
            int? comercioFinal = idComercio;

            if (User.IsInRole("Cajero"))
            {
                comercioFinal = await GetIdComercioDelUsuarioAsync();
                if (comercioFinal is null) return Forbid();
            }
            else if (User.IsInRole("Administrador"))
            {
                if (comercioFinal is null)
                {
                    TempData["Error"] = "Debe indicar el comercio.";
                    return RedirectToAction("Index", "Comercios");
                }
            }

            var nuevaCaja = new CajaModel { IdComercio = comercioFinal!.Value };
            ViewBag.IdComercio = comercioFinal;
            return View(nuevaCaja);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CajaModel caja)
        {
            // Seguridad: el cajero solo puede crear en su comercio
            if (User.IsInRole("Cajero"))
            {
                var idComercioUsuario = await GetIdComercioDelUsuarioAsync();
                if (idComercioUsuario is null || idComercioUsuario.Value != caja.IdComercio)
                    return Forbid();
            }

            ModelState.Remove("Comercio");
            if (!ModelState.IsValid)
            {
                ViewBag.IdComercio = caja.IdComercio;
                return View(caja);
            }

            var errorMessage = await _cajaService.CreateAsync(caja);
            if (errorMessage == null)
            {
                TempData["Success"] = "Caja registrada correctamente.";
                return RedirectToAction("Index", new { idComercio = caja.IdComercio });
            }

            ViewBag.IdComercio = caja.IdComercio;
            ViewBag.Error = errorMessage;
            return View(caja);
        }

        // GET /Caja/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var caja = await _cajaService.GetByIdAsync(id);
            if (caja == null)
            {
                TempData["Error"] = "Caja no encontrada.";
                return RedirectToAction("Index", "Comercios");
            }

            // Cajero solo si la caja pertenece a su comercio
            if (User.IsInRole("Cajero"))
            {
                var idComercioUsuario = await GetIdComercioDelUsuarioAsync();
                if (idComercioUsuario is null || idComercioUsuario.Value != caja.IdComercio)
                    return Forbid();
            }

            return View(caja);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(CajaModel caja)
        {
            // Cajero solo si la caja pertenece a su comercio
            if (User.IsInRole("Cajero"))
            {
                var idComercioUsuario = await GetIdComercioDelUsuarioAsync();
                if (idComercioUsuario is null || idComercioUsuario.Value != caja.IdComercio)
                    return Forbid();
            }

            ModelState.Remove("Comercio");

            if (!ModelState.IsValid) return View(caja);

            var ok = await _cajaService.UpdateAsync(caja);
            if (!ok)
            {
                ViewBag.Error = "No se pudo actualizar la caja.";
                return View(caja);
            }

            TempData["Success"] = "Caja actualizada correctamente.";
            return RedirectToAction("Index", new { idComercio = caja.IdComercio });
        }

        // GET /Caja/VerSinpe?idCaja=#
        public async Task<IActionResult> VerSinpe(int idCaja)
        {
            var caja = await _cajaService.GetByIdAsync(idCaja);
            if (caja == null)
            {
                TempData["Error"] = "Caja no encontrada.";
                return RedirectToAction("Index", "Comercios");
            }

            if (User.IsInRole("Cajero"))
            {
                var idComercioUsuario = await GetIdComercioDelUsuarioAsync();
                if (idComercioUsuario is null || idComercioUsuario.Value != caja.IdComercio)
                    return Forbid();
            }

            var pagos = await _sinpeService.ListarPorCajaAsync(idCaja);
            ViewBag.Caja = caja;
            return View(pagos);
        }

        // POST /Caja/Sincronizar?idSinpe=&idCaja=
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sincronizar(int idSinpe, int idCaja)
        {
            var caja = await _cajaService.GetByIdAsync(idCaja);
            if (caja == null)
            {
                TempData["Error"] = "Caja no encontrada.";
                return RedirectToAction("Index", "Comercios");
            }

            if (User.IsInRole("Cajero"))
            {
                var idComercioUsuario = await GetIdComercioDelUsuarioAsync();
                if (idComercioUsuario is null || idComercioUsuario.Value != caja.IdComercio)
                    return Forbid();
            }

            var ok = await _sinpeService.SincronizarAsync(idSinpe);
            TempData[ok ? "Success" : "Error"] = ok
                ? "SINPE sincronizado correctamente."
                : "No se pudo sincronizar el SINPE.";

            return RedirectToAction("VerSinpe", new { idCaja });
        }
    }
}
