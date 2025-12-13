using Los_Patitos.Models;

namespace Los_Patitos.Business
{
    public interface ISinpeService
    {
        Task<(bool ok, string error, int? idSinpe)> RegistrarAsync(SinpeModel input);
        Task<List<SinpeModel>> ListarPorCajaAsync(int idCaja);
        Task<List<SinpeModel>> ListarPorTelefonoCajaAsync(string telefonoCaja);
        Task<bool> SincronizarAsync(int idSinpe);

    }
}