using Los_Patitos.Business;
using Los_Patitos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Los_Patitos.Controllers
{
    public class CajaController : Controller
    {
        private readonly ICajaService _cajaService;
        private readonly ISinpeService _sinpeService;

        public CajaController(ICajaService cajaService, ISinpeService sinpeService)
        {
            _cajaService = cajaService;
            _sinpeService = sinpeService;
        }

        public async Task<IActionResult> Index(int idComercio)
        {
            var cajas = await _cajaService.GetByComercioAsync(idComercio);
            ViewBag.IdComercio = idComercio;
            return View(cajas);
        }


        //GET
        public IActionResult Crear(int idComercio)
        {
            var nuevaCaja = new CajaModel { IdComercio = idComercio };
            ViewBag.IdComercio = idComercio;
            return View(nuevaCaja);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CajaModel caja)
        {

            ModelState.Remove("Comercio");

            if (ModelState.IsValid)
            {
                var errorMessage = await _cajaService.CreateAsync(caja);

                if (errorMessage == null)
                {
                    // Mensaje Éxito
                    TempData["Success"] = "Caja registrada correctamente.";
                    return RedirectToAction("Index", new { idComercio = caja.IdComercio });
                }

                // Mensaje Error
                ViewBag.Error = errorMessage;
            }

            ViewBag.IdComercio = caja.IdComercio;
            return View(caja);
        }


        //GET
        public async Task<IActionResult> Editar(int id)
        {
            var caja = await _cajaService.GetByIdAsync(id);
            if (caja == null)
            {
                TempData["Error"] = "Caja no encontrada.";
                return RedirectToAction("Index", "Comercio");
            }
            return View(caja);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(CajaModel caja)
        {

            ModelState.Remove("Comercio");

            if (ModelState.IsValid)
            {
                var result = await _cajaService.UpdateAsync(caja);
                if (result)
                {
                    TempData["Success"] = "Caja actualizada correctamente.";
                    return RedirectToAction("Index", new { idComercio = caja.IdComercio });
                }

                ViewBag.Error = "No se pudo actualizar la caja.";
            }

            return View(caja);
        }

        //Ver Sinpes por caja
        public async Task<IActionResult> VerSinpe(int idCaja)
        {
            var caja = await _cajaService.GetByIdAsync(idCaja);
            if (caja == null)
            {
                TempData["Error"] = "Caja no encontrada.";
                return RedirectToAction("Index", "Comercio");
            }

            var pagos = await _sinpeService.ListarPorCajaAsync(idCaja);
            ViewBag.Caja = caja;
            return View(pagos); //Views/Caja/VerSinpe
        }
    }
}
