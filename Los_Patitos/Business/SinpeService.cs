using Los_Patitos.Models;
using Los_Patitos.Repositories;
using System.Text.RegularExpressions;

namespace Los_Patitos.Business
{
    public class SinpeService : ISinpeService
    {
        private readonly ISinpeRepository _sinpeRepo;
        private readonly ICajaRepository _cajaRepo;

        public SinpeService(ISinpeRepository sinpeRepo, ICajaRepository cajaRepo)
        {
            _sinpeRepo = sinpeRepo;
            _cajaRepo = cajaRepo;
        }

        public async Task<(bool ok, string? error, int? idSinpe)> RegistrarAsync(SinpeModel input)
        {
            //Validaciones 
            if (string.IsNullOrWhiteSpace(input.TelefonoOrigen) || !EsTelefonoValido(input.TelefonoOrigen))
                return (false, "El teléfono debe tener 8 dígitos.", null);

            if (string.IsNullOrWhiteSpace(input.NombreOrigen))
                return (false, "Campo obligatorio.", null);

            if (string.IsNullOrWhiteSpace(input.TelefonoDestinaria) || !EsTelefonoValido(input.TelefonoDestinaria))
                return (false, "El teléfono debe tener 8 dígitos.", null);

            if (string.IsNullOrWhiteSpace(input.NombreDestinaria))
                return (false, "Campo obligatorio.", null);

            if (input.Monto <= 0)
                return (false, "El monto debe ser mayor a cero.", null);

            //Validaciones de caja y teléfono
            var caja = await _cajaRepo.ObtenerPorTelefonoAsync(input.TelefonoDestinaria);
            if (caja is null)
                return (false, "No hay ninguna caja con ese teléfono registrada.", null);

            if (!caja.Estado)
                return (false, "Error, caja inactiva.", null);

            //Inputs del sistema
            input.IdCaja = caja.IdCaja;
            input.Estado = false;
            input.FechaDeRegistro = DateTime.Now;

            var id = await _sinpeRepo.CrearAsync(input);
            return (true, null, id);
        }

        public Task<List<SinpeModel>> ListarPorCajaAsync(int idCaja)
            => _sinpeRepo.ListarPorCajaAsync(idCaja);

        public async Task<List<SinpeModel>> ListarPorTelefonoCajaAsync(string telefonoCaja)
        {
            //BUscar la caja por el numero de telefono
            var caja = await _cajaRepo.ObtenerPorTelefonoAsync(telefonoCaja);
            if (caja is null)
            {
                //Si no hay ninguna caja con ese numero, devuelve lista vacia
                return new List<SinpeModel>();
            }

            //Se reutiliza buscar por id
            return await _sinpeRepo.ListarPorCajaAsync(caja.IdCaja);
        }

        private static bool EsTelefonoValido(string tel)
            => Regex.IsMatch(tel, @"^\d{8}$");


        public async Task<bool> SincronizarAsync(int idSinpe)
        {
            return await _sinpeRepo.SincronizarAsync(idSinpe);
        }

    }
}
