using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Los_Patitos.Business;
using Los_Patitos.Models;

namespace Los_Patitos.Controllers
{
    public class SinpeController : Controller
    {
        private readonly ISinpeService _sinpeService;

        public SinpeController(ISinpeService sinpeService)
        {
            _sinpeService = sinpeService;
        }

        //GET Crear
        public IActionResult Crear()
        {
            return View(new SinpeModel());
        }

        //POST Crear
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Crear([Bind(
            "TelefonoOrigen,NombreOrigen,TelefonoDestinaria,NombreDestinaria,Monto,Descripcion"
        )] SinpeModel modelo)
        {
            ModelState.Remove(nameof(SinpeModel.Caja));

            if (!ModelState.IsValid)
                return View(modelo);

            var (ok, error, id) = await _sinpeService.RegistrarAsync(modelo);

            if (!ok)
            {
                ViewBag.Error = error;
                return View(modelo);
            }

            TempData["Ok"] = $"SINPE registrado con el Id #{id}.";
            return RedirectToAction(nameof(Crear));
        }
    }
}
