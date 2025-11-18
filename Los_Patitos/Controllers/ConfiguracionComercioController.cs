using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Los_Patitos.Business;
using Los_Patitos.Models;

namespace Los_Patitos.Controllers
{
    public class ConfiguracionComercioController : Controller
    {
        private readonly IConfiguracionComercioService _configs;
        private readonly IComercioService _comercios;
        private readonly IBitacoraService _bitacora;

        public ConfiguracionComercioController(
            IConfiguracionComercioService configs,
            IComercioService comercios,
            IBitacoraService bitacora)
        {
            _configs = configs;
            _comercios = comercios;
            _bitacora = bitacora;
        }

        // LISTAR
        public IActionResult Index()
        {
            var lista = _configs.Listar();
            return View(lista);
        }

        // CREAR (GET) se entra desde Comercios con ?idComercio=#
        public IActionResult Crear(int idComercio)
        {
            try
            {
                var comercio = _comercios.Detalle(idComercio);

                var existente = _configs.ObtenerPorComercio(idComercio);
                if (existente != null)
                {
                    TempData["Error"] = "Ya existe una configuración para este comercio.";
                    return RedirectToAction(nameof(Editar), new { id = existente.IdConfiguracion });
                }

                ViewBag.NombreComercio = comercio.Nombre;
                CargarCombos();

                var modelo = new ConfiguracionComercio
                {
                    IdComercio = idComercio
                };

                return View(modelo);
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Comercio");
            }
        }

        // CREAR (POST)
        [HttpPost]
        public async Task<IActionResult> Crear(ConfiguracionComercio modelo)
        {
            if (!ModelState.IsValid)
            {
                CargarCombos();
                return View(modelo);
            }

            try
            {
                _configs.Registrar(modelo);
                TempData["Ok"] = "Configuración registrada.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex) // ya existe config
            {
                await LogErrorAsync("ConfiguracionComercio",
                    $"Intento de configuración duplicada. Ruta: {Request?.Path}",
                    ex);

                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Crear), new { idComercio = modelo.IdComercio });
            }
            catch (KeyNotFoundException ex)
            {
                await LogErrorAsync("ConfiguracionComercio",
                    $"Comercio no encontrado en Crear. Ruta: {Request?.Path}",
                    ex);

                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Comercio");
            }
            catch (DbUpdateException dbex) when (dbex.InnerException is MySqlException mysql && mysql.Number == 1062)
            {
                await LogErrorAsync("ConfiguracionComercio",
                    $"Violación UNIQUE (1062) en Crear. Ruta: {Request?.Path}",
                    dbex);

                TempData["Error"] = "Ya existe una configuración para este comercio.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await LogErrorAsync("ConfiguracionComercio",
                    $"Error inesperado en Crear. Ruta: {Request?.Path}",
                    ex);

                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado.");
                CargarCombos();
                return View(modelo);
            }
        }

        // EDITAR (GET)
        public IActionResult Editar(int id)
        {
            try
            {
                var config = _configs.Detalle(id);
                ViewBag.NombreComercio = config.Comercio?.Nombre;
                CargarCombos(config.TipoConfiguracion, config.Estado);
                return View(config);
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // EDITAR (POST)
        [HttpPost]
        public async Task<IActionResult> Editar(int id, ConfiguracionComercio modelo)
        {
            if (id != modelo.IdConfiguracion) return BadRequest();

            if (!ModelState.IsValid)
            {
                CargarCombos(modelo.TipoConfiguracion, modelo.Estado);
                return View(modelo);
            }

            try
            {
                _configs.Actualizar(modelo);
                TempData["Ok"] = "Configuración actualizada.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbex) when (dbex.InnerException is MySqlException mysql && mysql.Number == 1062)
            {
                await LogErrorAsync("ConfiguracionComercio",
                    $"Violación UNIQUE (1062) en Editar. Ruta: {Request?.Path}",
                    dbex);

                ModelState.AddModelError(string.Empty, "Ya existe una configuración para este comercio.");
                CargarCombos(modelo.TipoConfiguracion, modelo.Estado);
                return View(modelo);
            }
            catch (KeyNotFoundException ex)
            {
                await LogErrorAsync("ConfiguracionComercio",
                    $"Config no encontrada en Editar. Ruta: {Request?.Path}",
                    ex);

                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await LogErrorAsync("ConfiguracionComercio",
                    $"Error inesperado en Editar. Ruta: {Request?.Path}",
                    ex);

                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado.");
                CargarCombos(modelo.TipoConfiguracion, modelo.Estado);
                return View(modelo);
            }
        }

        // combos para tipo y estado
        private void CargarCombos(int? tipoSel = null, bool? estadoSel = null)
        {
            ViewBag.TiposConfiguracion = new SelectList(new[]
            {
                new { Id = 1, Nombre = "Plataforma" },
                new { Id = 2, Nombre = "Externa" },
                new { Id = 3, Nombre = "Ambas" }
            }, "Id", "Nombre", tipoSel);

            ViewBag.Estados = new SelectList(new[]
            {
                new { Key = true, Value = "Activo" },
                new { Key = false, Value = "Inactivo" }
            }, "Key", "Value", estadoSel.HasValue ? estadoSel.Value : true);
        }

        private async Task LogErrorAsync(string tabla, string descripcion, Exception ex, CancellationToken ct = default)
        {
            await _bitacora.EscribirAsync(new BitacoraEvento
            {
                TablaDeEvento = tabla,
                TipoDeEvento = "Error",
                FechaDeEvento = DateTime.Now,
                DescripcionDeEvento = descripcion,
                StackTrace = ex.ToString(),
                DatosAnteriores = null,
                DatosPosteriores = null
            }, ct);
        }
    }
}
