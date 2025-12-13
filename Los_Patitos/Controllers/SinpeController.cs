using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Los_Patitos.Business;
using Los_Patitos.Models;


namespace Los_Patitos.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class SinpeController : Controller
    {
        private readonly ISinpeService _sinpeService;
        private readonly IHttpClientFactory _httpFactory;
        private readonly string _apiBase;

        // ==== DTOs internos para hablar con el API externo ====
        private record AuthRequest(int IdComercio);
        private record AuthResponse(bool EsValido, string? Mensaje, string? Token, DateTime? ExpiraUtc);

        private record SinpeConsultaDto(
            int IdSinpe,
            string TelefonoOrigen,
            string NombreOrigen,
            string TelefonoDestinatario,
            string NombreDestinatario,
            decimal Monto,
            string? Descripcion,
            DateTime Fecha,
            bool Estado
        );

        private record SinpeSincronizarRequest(int IdSinpe);
        private record ApiResult(bool EsValido, string Mensaje);

        private record SinpeRecibirRequest(
            string TelefonoOrigen,
            string NombreOrigen,
            string TelefonoDestinatario,
            string NombreDestinatario,
            decimal Monto,
            string? Descripcion
        );

        public SinpeController(ISinpeService sinpeService, IHttpClientFactory httpFactory, IConfiguration cfg)
        {
            _sinpeService = sinpeService;
            _httpFactory = httpFactory;
            _apiBase = cfg["ApiExterno:BaseUrl"] ?? throw new InvalidOperationException("Falta ApiExterno:BaseUrl en appsettings.json");
        }

        // ================== UI LOCAL (avance 1) ==================
        public IActionResult Crear()
        {
            return View(new SinpeModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([Bind("TelefonoOrigen,NombreOrigen,TelefonoDestinaria,NombreDestinaria,Monto,Descripcion")] SinpeModel modelo)
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

        // ================== UI PARA PROBAR EL API ==================
        // GET /Sinpe/Api -> tu vista con formularios para consultar/sincronizar/recibir
        public IActionResult Api()
        {
            return View();
        }

        // POST /Sinpe/ConsultarApi
        // Campos esperados en el form: telefonoCaja, idComercio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConsultarApi(string telefonoCaja, int idComercio)
        {
            if (string.IsNullOrWhiteSpace(telefonoCaja))
            {
                TempData["ApiError"] = "Debe indicar el teléfono de la caja.";
                return RedirectToAction(nameof(Api));
            }

            var token = await GetTokenAsync(idComercio);
            if (token is null)
            {
                TempData["ApiError"] = "No se pudo obtener el token. Verifique IdComercio y su configuración (Externa/Ambas, Activo).";
                return RedirectToAction(nameof(Api));
            }

            try
            {
                var client = _httpFactory.CreateClient();
                client.BaseAddress = new Uri(_apiBase);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var url = $"/api/sinpe/consultar?telefonoCaja={Uri.EscapeDataString(telefonoCaja)}";
                var resp = await client.GetAsync(url);

                if (!resp.IsSuccessStatusCode)
                {
                    TempData["ApiError"] = $"Error {((int)resp.StatusCode)} al consultar SINPE.";
                    return RedirectToAction(nameof(Api));
                }

                var json = await resp.Content.ReadAsStringAsync();
                var lista = JsonSerializer.Deserialize<List<SinpeConsultaDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

                // Para mostrar en la vista
                TempData["ApiOk"] = $"Consulta OK. Se recibieron {lista.Count} registros.";
                TempData["ApiJson"] = json;
            }
            catch (Exception ex)
            {
                TempData["ApiError"] = "Excepción al consultar SINPE: " + ex.Message;
            }

            return RedirectToAction(nameof(Api));
        }

        // POST /Sinpe/SincronizarApi
        // Campos esperados: idSinpe, idComercio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SincronizarApi(int idSinpe, int idComercio)
        {
            if (idSinpe <= 0)
            {
                TempData["ApiError"] = "IdSinpe inválido.";
                return RedirectToAction(nameof(Api));
            }

            var token = await GetTokenAsync(idComercio);
            if (token is null)
            {
                TempData["ApiError"] = "No se pudo obtener el token (IdComercio/configuración).";
                return RedirectToAction(nameof(Api));
            }

            try
            {
                var client = _httpFactory.CreateClient();
                client.BaseAddress = new Uri(_apiBase);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var body = new SinpeSincronizarRequest(idSinpe);
                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

                var resp = await client.PostAsync("/api/sinpe/sincronizar", content);

                var json = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode)
                {
                    TempData["ApiError"] = $"Error {((int)resp.StatusCode)} al sincronizar SINPE. Respuesta: {json}";
                    return RedirectToAction(nameof(Api));
                }

                var result = JsonSerializer.Deserialize<ApiResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result is null)
                {
                    TempData["ApiError"] = "No se pudo interpretar la respuesta del API.";
                }
                else
                {
                    TempData["ApiOk"] = $"SincronizarApi → {result.Mensaje}";
                    TempData["ApiJson"] = json;
                }
            }
            catch (Exception ex)
            {
                TempData["ApiError"] = "Excepción al sincronizar SINPE: " + ex.Message;
            }

            return RedirectToAction(nameof(Api));
        }

        // POST /Sinpe/RecibirApi
        // Campos esperados: IdComercio y los del formulario de registro (origen/destino/monto/descripcion)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecibirApi([Bind("TelefonoOrigen,NombreOrigen,TelefonoDestinaria,NombreDestinaria,Monto,Descripcion")] SinpeModel modelo, int idComercio)
        {
            if (!ModelState.IsValid)
            {
                TempData["ApiError"] = "Formulario inválido para Recibir SINPE.";
                return RedirectToAction(nameof(Api));
            }

            var token = await GetTokenAsync(idComercio);
            if (token is null)
            {
                TempData["ApiError"] = "No se pudo obtener el token (IdComercio/configuración).";
                return RedirectToAction(nameof(Api));
            }

            try
            {
                var req = new SinpeRecibirRequest(
                    TelefonoOrigen: modelo.TelefonoOrigen,
                    NombreOrigen: modelo.NombreOrigen,
                    TelefonoDestinatario: modelo.TelefonoDestinaria,
                    NombreDestinatario: modelo.NombreDestinaria,
                    Monto: modelo.Monto,
                    Descripcion: modelo.Descripcion
                );

                var client = _httpFactory.CreateClient();
                client.BaseAddress = new Uri(_apiBase);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var content = new StringContent(JsonSerializer.Serialize(req), Encoding.UTF8, "application/json");
                var resp = await client.PostAsync("/api/sinpe/recibir", content);

                var json = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode)
                {
                    TempData["ApiError"] = $"Error {((int)resp.StatusCode)} al recibir SINPE. Respuesta: {json}";
                    return RedirectToAction(nameof(Api));
                }

                var result = JsonSerializer.Deserialize<ApiResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result is null)
                {
                    TempData["ApiError"] = "No se pudo interpretar la respuesta del API.";
                }
                else
                {
                    TempData["ApiOk"] = $"RecibirApi → {result.Mensaje}";
                    TempData["ApiJson"] = json;
                }
            }
            catch (Exception ex)
            {
                TempData["ApiError"] = "Excepción al recibir SINPE: " + ex.Message;
            }

            return RedirectToAction(nameof(Api));
        }

        // ================== HELPER PARA OBTENER TOKEN ==================
        private async Task<string?> GetTokenAsync(int idComercio)
        {
            if (idComercio <= 0) return null;

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
