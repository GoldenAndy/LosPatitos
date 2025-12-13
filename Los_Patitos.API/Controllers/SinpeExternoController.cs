using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Los_Patitos.Business;
using Los_Patitos.Models;

namespace Los_Patitos.API.Controllers
{
    [ApiController]
    [Route("api/sinpe")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SinpeExternoController : ControllerBase
    {
        private readonly ISinpeService _sinpe;
        private readonly IConfiguracionComercioService _config;

        public SinpeExternoController(ISinpeService sinpe, IConfiguracionComercioService config)
        {
            _sinpe = sinpe;
            _config = config;
        }

        // ===== DTOs requeridos por el enunciado =====
        public record SinpeConsultaDto(
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

        public record RespuestaOp(bool EsValido, string Mensaje);

        public record SincronizarReq([Required] int IdSinpe);

        public record RecibirReq(
            [Required] string TelefonoOrigen,
            [Required] string NombreOrigen,
            [Required] string TelefonoDestinatario,
            [Required] string NombreDestinatario,
            [Range(0.01, double.MaxValue)] decimal Monto,
            [Required] string Descripcion
        );

        // ===== Helpers =====
        private int? GetIdComercio()
        {
            var c = User.FindFirst("idComercio") ?? User.FindFirst(JwtRegisteredClaimNames.Sub);
            return int.TryParse(c?.Value, out var id) ? id : null;
        }

        private static bool PermiteExterna(ConfiguracionComercio cfg)
            => cfg.Estado && (cfg.TipoConfiguracion == 2 || cfg.TipoConfiguracion == 3);

        private ActionResult? CheckExterna(int idComercio)
        {
            var cfg = _config.ObtenerPorComercio(idComercio);
            if (cfg is null || !PermiteExterna(cfg))
                return Unauthorized(new RespuestaOp(false, "Comercio no autorizado para uso externo."));
            return null;
        }

        // ===== 1) Consultar SINPE =====
        // GET api/sinpe/consultar?telefonoCaja=8XXXXXXXX
        [HttpGet("consultar")]
        public async Task<ActionResult<IEnumerable<SinpeConsultaDto>>> Consultar([FromQuery] string telefonoCaja)
        {
            var idComercio = GetIdComercio();
            if (idComercio is null)
                return Unauthorized(new RespuestaOp(false, "Token sin idComercio."));

            var auth = CheckExterna(idComercio.Value);
            if (auth is not null) return auth;

            if (string.IsNullOrWhiteSpace(telefonoCaja))
                return BadRequest(new RespuestaOp(false, "Debe indicar telefonoCaja."));

            var lista = await _sinpe.ListarPorTelefonoCajaAsync(telefonoCaja);

            var dto = lista.Select(s => new SinpeConsultaDto(
                IdSinpe: s.IdSinpe,
                TelefonoOrigen: s.TelefonoOrigen,
                NombreOrigen: s.NombreOrigen,
                TelefonoDestinatario: s.TelefonoDestinaria,   // map a tu nombre real
                NombreDestinatario: s.NombreDestinaria,       // map a tu nombre real
                Monto: s.Monto,
                Descripcion: s.Descripcion,
                Fecha: s.FechaDeRegistro,
                Estado: s.Estado
            ));

            return Ok(dto);
        }

        // ===== 2) Sincronizar SINPE =====
        // POST api/sinpe/sincronizar  { "idSinpe": 123 }
        [HttpPost("sincronizar")]
        public async Task<ActionResult<RespuestaOp>> Sincronizar([FromBody] SincronizarReq req)
        {
            var idComercio = GetIdComercio();
            if (idComercio is null)
                return Unauthorized(new RespuestaOp(false, "Token sin idComercio."));

            var auth = CheckExterna(idComercio.Value);
            if (auth is not null) return auth;

            if (!ModelState.IsValid)
                return BadRequest(new RespuestaOp(false, "Solicitud inválida."));

            try
            {
                var ok = await _sinpe.SincronizarAsync(req.IdSinpe);
                return ok
                    ? Ok(new RespuestaOp(true, "SINPE sincronizado correctamente."))
                    : BadRequest(new RespuestaOp(false, "No fue posible sincronizar el SINPE."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new RespuestaOp(false, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new RespuestaOp(false, ex.Message));
            }
            catch
            {
                return StatusCode(500, new RespuestaOp(false, "Error interno al sincronizar."));
            }
        }

        // ===== 3) Recibir SINPE =====
        // POST api/sinpe/recibir
        [HttpPost("recibir")]
        public async Task<ActionResult<RespuestaOp>> Recibir([FromBody] RecibirReq req)
        {
            var idComercio = GetIdComercio();
            if (idComercio is null)
                return Unauthorized(new RespuestaOp(false, "Token sin idComercio."));

            var auth = CheckExterna(idComercio.Value);
            if (auth is not null) return auth;

            if (!ModelState.IsValid)
                return BadRequest(new RespuestaOp(false, "Solicitud inválida."));

            try
            {
                var nuevo = new SinpeModel
                {
                    TelefonoOrigen = req.TelefonoOrigen,
                    NombreOrigen = req.NombreOrigen,
                    TelefonoDestinaria = req.TelefonoDestinatario, // map a tus props
                    NombreDestinaria = req.NombreDestinatario,     // map a tus props
                    Monto = req.Monto,
                    Descripcion = req.Descripcion,
                    Estado = false
                };

                var (ok, error, id) = await _sinpe.RegistrarAsync(nuevo);
                return ok
                    ? Ok(new RespuestaOp(true, $"SINPE recibido. Id={id}"))
                    : BadRequest(new RespuestaOp(false, error ?? "No fue posible registrar el SINPE."));
            }
            catch
            {
                return StatusCode(500, new RespuestaOp(false, "Error interno al registrar SINPE."));
            }
        }
    }
}
