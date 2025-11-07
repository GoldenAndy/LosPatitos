using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;                 // Esto sirve para mitigar duplicados
using Los_Patitos.Business;
using Los_Patitos.Models;

namespace Los_Patitos.Controllers
{
    public class ComercioController : Controller
    {
        private readonly IComercioService _comercios;
        private readonly ITipoIdentificacionService _tiposIdentificacion;
        private readonly ITipoComercioService _tiposComercio;
        private readonly IBitacoraService _bitacora; // service de bitácora para guardar errores con stacktrace

        public ComercioController(
            IComercioService comercios,
            ITipoIdentificacionService tiposIdentificacion,
            ITipoComercioService tiposComercio,
            IBitacoraService bitacora) 
        {
            _comercios = comercios;
            _tiposIdentificacion = tiposIdentificacion;
            _tiposComercio = tiposComercio;
            _bitacora = bitacora; 
        }

        // =========================
        // LISTAR
        // =========================
        public IActionResult Index()
        {
            var lista = _comercios.Listar();
            return View(lista);
        }

        // =========================
        // DETALLES
        // =========================
        public IActionResult Detalles(int id)
        {
            try
            {
                var comercio = _comercios.Detalle(id);
                return View(comercio);
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // =========================
        // CREAR (GET)
        // =========================
        public IActionResult Crear()
        {
            CargarOpciones(incluirEstado: false);
            return View(new Comercio());
        }

        // =========================
        // CREAR (POST)
        // =========================
        [HttpPost]
        public async Task<IActionResult> Crear(Comercio modelo) 
        {
            if (!ModelState.IsValid)
            {
                CargarOpciones(incluirEstado: false);
                return View(modelo);
            }
            try
            {
                _comercios.Registrar(modelo);
                TempData["Ok"] = "Comercio registrado.";
                return RedirectToAction(nameof(Index));
            }
            // 1062 = Duplicate entry (índice UNIQUE violado)
            catch (DbUpdateException dbex) when (dbex.InnerException is MySqlException mysql && mysql.Number == 1062)
            {
                // escribir stacktrace en bitácora
                await LogErrorAsync("Comercio",
                    $"Violación de UNIQUE (1062) en Crear. Ruta: {Request?.Path}",
                    dbex);

                ModelState.AddModelError(nameof(Comercio.Identificacion), "Ya existe un comercio con esa identificación.");
                CargarOpciones(incluirEstado: false);
                return View(modelo);
            }
            catch (KeyNotFoundException ex)
            {
                await LogErrorAsync("Comercio",
                    $"Clave no encontrada en Crear. Ruta: {Request?.Path}",
                    ex);

                ModelState.AddModelError(string.Empty, ex.Message);
                CargarOpciones(incluirEstado: false);
                return View(modelo);
            }
            catch (Exception ex) 
            {
                await LogErrorAsync("Comercio",
                    $"Error inesperado en Crear. Ruta: {Request?.Path}",
                    ex);

                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado.");
                CargarOpciones(incluirEstado: false);
                return View(modelo);
            }
        }

        // =========================
        // EDITAR (GET)
        // =========================
        public IActionResult Editar(int id)
        {
            try
            {
                var comercio = _comercios.Detalle(id);
                CargarOpciones(incluirEstado: true, comercio.TipoIdentificacion, comercio.TipoComercio, comercio.Estado);
                return View(comercio);
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // =========================
        // EDITAR (POST)
        // =========================
        [HttpPost]
        public async Task<IActionResult> Editar(int id, Comercio modelo)
        {
            if (id != modelo.IdComercio) return BadRequest();

            if (!ModelState.IsValid)
            {

                CargarOpciones(incluirEstado: true, modelo.TipoIdentificacion, modelo.TipoComercio, modelo.Estado);
                return View(modelo);
            }

            try
            {
                _comercios.Actualizar(modelo);
                TempData["Ok"] = "Comercio actualizado.";
                return RedirectToAction(nameof(Index));
            }
            // 1062 = Duplicate entry
            catch (DbUpdateException dbex) when (dbex.InnerException is MySqlException mysql && mysql.Number == 1062)
            {
                await LogErrorAsync("Comercio",
                    $"Violación de UNIQUE (1062) en Editar. Ruta: {Request?.Path}",
                    dbex);

                ModelState.AddModelError(nameof(Comercio.Identificacion), "Ya existe un comercio con esa identificación.");
                CargarOpciones(incluirEstado: true, modelo.TipoIdentificacion, modelo.TipoComercio, modelo.Estado);
                return View(modelo);
            }
            catch (KeyNotFoundException ex)
            {
                await LogErrorAsync("Comercio",
                    $"Clave no encontrada en Editar. Ruta: {Request?.Path}",
                    ex);

                ModelState.AddModelError(string.Empty, ex.Message);
                CargarOpciones(incluirEstado: true, modelo.TipoIdentificacion, modelo.TipoComercio, modelo.Estado);
                return View(modelo);
            }
            catch (Exception ex) 
            {
                await LogErrorAsync("Comercio",
                    $"Error inesperado en Editar. Ruta: {Request?.Path}",
                    ex);

                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado.");
                CargarOpciones(incluirEstado: true, modelo.TipoIdentificacion, modelo.TipoComercio, modelo.Estado);
                return View(modelo);
            }
        }

        // =========================
        // Cargar opciones de selects
        // =========================
        private void CargarOpciones(bool incluirEstado, int? tipoIdentSel = null, int? tipoComSel = null, bool? estadoSel = null)
        {
            var tiposId = _tiposIdentificacion.Listar();
            var tiposCom = _tiposComercio.Listar();

            ViewBag.TiposIdentificacion = new SelectList(tiposId, "Id", "Nombre", tipoIdentSel);
            ViewBag.TiposComercio = new SelectList(tiposCom, "Id", "Nombre", tipoComSel);

            if (incluirEstado)
            {
                ViewBag.Estados = new SelectList(new[]
                {
                    new { Key = true,  Value = "Activo"   },
                    new { Key = false, Value = "Inactivo" }
                }, "Key", "Value", estadoSel.HasValue ? estadoSel.Value : true);
            }
        }

        // =========================
        // Helper para escribir errores con stacktrace en bitácora
        // =========================
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
