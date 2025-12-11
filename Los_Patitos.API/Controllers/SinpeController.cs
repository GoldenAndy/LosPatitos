using Microsoft.AspNetCore.Mvc;
using Los_Patitos.API.Dtos;
using Los_Patitos.Business;
using Los_Patitos.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Los_Patitos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SinpeController : ControllerBase
    {
        private readonly ISinpeService _sinpeService;

        public SinpeController(ISinpeService sinpeService)
        {
            _sinpeService = sinpeService;
        }

        // GET api/sinpe?telefonoCaja=88887777
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SinpeDto>>> Consultar([FromQuery] string telefonoCaja)
        {
            if (string.IsNullOrWhiteSpace(telefonoCaja))
                return BadRequest("El teléfono de la caja es obligatorio.");

            //Método en ISinpeService y SinpeService
            var lista = await _sinpeService.ListarPorTelefonoCajaAsync(telefonoCaja);

            var resultado = lista.Select(s => new SinpeDto
            {
                IdSinpe = s.IdSinpe,
                TelefonoOrigen = s.TelefonoOrigen,
                NombreOrigen = s.NombreOrigen,
                TelefonoDestinatario = s.TelefonoDestinaria,
                NombreDestinatario = s.NombreDestinaria,
                Monto = s.Monto,
                Descripcion = s.Descripcion,
                Fecha = s.FechaDeRegistro,
                Estado = s.Estado
            }).ToList();

            return Ok(resultado);
        }

        // POST api/sinpe/sincronizar
        [HttpPost("sincronizar")]
        public async Task<ActionResult<ApiResultadoDto>> Sincronizar([FromBody] SincronizarSinpeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResultadoDto
                {
                    EsValido = false,
                    Mensaje = "Datos inválidos."
                });
            }

            //SinpeService.SincronizarAsync devuelve un booleano
            bool ok = await _sinpeService.SincronizarAsync(request.IdSinpe);
            string mensaje = ok
                ? "SINPE sincronizado correctamente."
                : "No se pudo sincronizar el SINPE.";

            return Ok(new ApiResultadoDto
            {
                EsValido = ok,
                Mensaje = mensaje
            });
        }

        // POST api/sinpe/recibir
        [HttpPost("recibir")]
        public async Task<ActionResult<ApiResultadoDto>> Recibir([FromBody] RecibirSinpeRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errores = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

                return BadRequest(new ApiResultadoDto { EsValido = false, Mensaje = $"Datos inválidos: {errores}" });
            }

            var modelo = new SinpeModel
            {
                TelefonoOrigen = request.TelefonoOrigen, NombreOrigen = request.NombreOrigen, TelefonoDestinaria = request.TelefonoDestinatario, NombreDestinaria = request.NombreDestinatario,  Monto = request.Monto,  Descripcion = request.Descripcion, 
                FechaDeRegistro = DateTime.Now, Estado = false
            };

            //SinpeService.RegistrarAsync devuelve (bool ok, string error, int? idSinpe)
            var resultado = await _sinpeService.RegistrarAsync(modelo);
            bool ok = resultado.ok;
            string error = resultado.error;
            int? idSinpe = resultado.idSinpe;

            return Ok(new ApiResultadoDto { EsValido = ok, Mensaje = ok ? $"SINPE registrado con el Id #{idSinpe}." : (string.IsNullOrWhiteSpace(error) ? "Ocurrió un error al registrar el SINPE." : error) });
        }
    }
}
