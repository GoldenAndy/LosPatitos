using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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
        private readonly IUsuarioService _usuarioService;
        private readonly UserManager<UsuarioIdentity> _userManager;

        private readonly IConfiguracionComercioService _configService;
        private readonly IHttpClientFactory _httpFactory;
        private readonly string _apiBase;

        private readonly ILogger<CajaController> _logger;

        // DTOs para API
        private record ApiResult(bool EsValido, string Mensaje);
        private record AuthRequest(int IdComercio);
        private record AuthResponse(bool EsValido, string? Mensaje, string? Token, DateTime? ExpiraUtc);
        private record SinpeSincronizarRequest(int IdSinpe);

        // DTO para el body JSON de /Caja/SincronizarAjax
        public record SincronizarAjaxRequest(int IdSinpe, int IdCaja);

        public CajaController(
            ICajaService cajaService,
            ISinpeService sinpeService,
            UserManager<UsuarioIdentity> userManager,
            IUsuarioService usuarioService,
            IConfiguracionComercioService configService,
            IHttpClientFactory httpFactory,
            IConfiguration cfg,
            ILogger<CajaController> logger)
        {
            _cajaService = cajaService;
            _sinpeService = sinpeService;
            _userManager = userManager;
            _usuarioService = usuarioService;

            _configService = configService;
            _httpFactory = httpFactory;
            _apiBase = cfg["ApiExterno:BaseUrl"] ?? throw new InvalidOperationException("Falta ApiExterno:BaseUrl");

            _logger = logger;
        }

        private async Task<int?> GetIdComercioDelUsuarioAsync()
        {
            var netUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(netUserId)) return null;
            var usuario = await _usuarioService.ObtenerPorIdNetUserAsync(netUserId);
            return usuario?.IdComercio;
        }

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
                    return RedirectToAction("Index", "Comercio");
                }
            }

            var cajas = await _cajaService.GetByComercioAsync(comercioFinal!.Value);
            ViewBag.IdComercio = comercioFinal;
            return View(cajas);
        }

        public async Task<IActionResult> VerSinpe(int idCaja)
        {
            var caja = await _cajaService.GetByIdAsync(idCaja);
            if (caja == null)
            {
                TempData["Error"] = "Caja no encontrada.";
                return RedirectToAction("Index", "Comercio");
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

        // === Sincronización AJAX con decisión Local vs API (y fallback si aplica) ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SincronizarAjax([FromBody] SincronizarAjaxRequest req)
        {
            if (req is null || req.IdSinpe <= 0 || req.IdCaja <= 0)
                return Json(new { esValido = false, mensaje = "Parámetros inválidos." });

            var caja = await _cajaService.GetByIdAsync(req.IdCaja);
            if (caja is null)
                return Json(new { esValido = false, mensaje = "Caja no encontrada." });

            // Seguridad: cajero solo en su comercio
            if (User.IsInRole("Cajero"))
            {
                var idComercioUsuario = await GetIdComercioDelUsuarioAsync();
                if (idComercioUsuario is null || idComercioUsuario.Value != caja.IdComercio)
                    return Json(new { esValido = false, mensaje = "Acceso denegado." });
            }

            // Leer configuración del comercio
            var config = _configService.ObtenerPorComercio(caja.IdComercio);
            if (config is null || !config.Estado)
                return Json(new { esValido = false, mensaje = "El comercio no tiene configuración activa." });

            try
            {
                // 1) SOLO PLATAFORMA (Local)
                if (config.TipoConfiguracion == 1)
                {
                    var ok = await _sinpeService.SincronizarAsync(req.IdSinpe);
                    _logger.LogInformation("SINPE {Id} sincronizado vía LOCAL para Comercio {Comercio}. Ok={Ok}",
                        req.IdSinpe, caja.IdComercio, ok);

                    return Json(new
                    {
                        esValido = ok,
                        mensaje = ok ? "SINPE sincronizado localmente." : "No se pudo sincronizar el SINPE.",
                        via = "LOCAL",
                        fallback = false
                    });
                }

                // 2) EXTERNO o AMBAS → intentar API primero
                if (config.TipoConfiguracion == 2 || config.TipoConfiguracion == 3)
                {
                    var token = await GetTokenAsync(caja.IdComercio);
                    if (token is not null)
                    {
                        var client = _httpFactory.CreateClient();
                        client.BaseAddress = new Uri(_apiBase);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                        var body = new SinpeSincronizarRequest(req.IdSinpe);
                        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

                        var resp = await client.PostAsync("/api/sinpe/sincronizar", content);
                        var json = await resp.Content.ReadAsStringAsync();

                        if (resp.IsSuccessStatusCode)
                        {
                            var result = JsonSerializer.Deserialize<ApiResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                                         ?? new ApiResult(false, "Respuesta inválida del API.");

                            _logger.LogInformation("SINPE {Id} → API externo. Ok={Ok}. Status={Status}. Mensaje={Msg}",
                                req.IdSinpe, result.EsValido, (int)resp.StatusCode, result.Mensaje);

                            if (result.EsValido)
                            {
                                return Json(new
                                {
                                    esValido = true,
                                    mensaje = result.Mensaje,
                                    via = "API",
                                    fallback = false
                                });
                            }

                            // Si es SOLO EXTERNA y el API dijo no → no hay fallback
                            if (config.TipoConfiguracion == 2)
                            {
                                return Json(new
                                {
                                    esValido = false,
                                    mensaje = result.Mensaje,
                                    via = "API",
                                    fallback = false
                                });
                            }
                            // Si es AMBAS, seguimos abajo al fallback local
                        }
                        else
                        {
                            _logger.LogWarning("SINPE {Id} → API externo FALLÓ. Status={Status}. Body={Body}",
                                req.IdSinpe, (int)resp.StatusCode, json);

                            if (config.TipoConfiguracion == 2)
                            {
                                return Json(new
                                {
                                    esValido = false,
                                    mensaje = $"API respondió {(int)resp.StatusCode}.",
                                    via = "API",
                                    fallback = false
                                });
                            }
                            // Si es AMBAS, seguimos abajo al fallback local
                        }
                    }
                    else
                    {
                        _logger.LogWarning("SINPE {Id} → No se pudo obtener token para API externo. Comercio={Comercio}",
                            req.IdSinpe, caja.IdComercio);

                        if (config.TipoConfiguracion == 2)
                        {
                            return Json(new
                            {
                                esValido = false,
                                mensaje = "No se pudo obtener token del API externo.",
                                via = "API",
                                fallback = false
                            });
                        }
                        // Si es AMBAS, seguimos al fallback local
                    }
                }

                // 3) Fallback LOCAL (solo si TipoConfiguracion = AMBAS = 3)
                if (config.TipoConfiguracion == 3)
                {
                    var ok = await _sinpeService.SincronizarAsync(req.IdSinpe);
                    _logger.LogInformation("SINPE {Id} sincronizado vía LOCAL (FALLBACK) para Comercio {Comercio}. Ok={Ok}",
                        req.IdSinpe, caja.IdComercio, ok);

                    return Json(new
                    {
                        esValido = ok,
                        mensaje = ok ? "Sincronizado localmente (fallback)." : "Fallback local falló.",
                        via = "LOCAL",
                        fallback = true
                    });
                }

                // Si llegamos aquí y no era 1/2/3 válido
                return Json(new { esValido = false, mensaje = "Configuración inválida.", via = "LOCAL", fallback = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al sincronizar SINPE {Id} para Caja {Caja}", req.IdSinpe, req.IdCaja);
                return Json(new { esValido = false, mensaje = "Error al sincronizar.", via = "LOCAL", fallback = false });
            }
        }

        // Helper para token del API
        private async Task<string?> GetTokenAsync(int idComercio)
        {
            try
            {
                var client = _httpFactory.CreateClient();
                client.BaseAddress = new Uri(_apiBase);

                var body = new AuthRequest(idComercio);
                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

                var resp = await client.PostAsync("/api/autenticacion/token", content);
                if (!resp.IsSuccessStatusCode) return null;

                var json = await resp.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<AuthResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (data is null || !data.EsValido || string.IsNullOrWhiteSpace(data.Token)) return null;
                return data.Token;
            }
            catch
            {
                return null;
            }
        }
    }
}
