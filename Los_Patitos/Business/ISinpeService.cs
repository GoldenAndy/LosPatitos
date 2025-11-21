using System.Threading.Tasks;
using Los_Patitos.Models;
using System.Collections.Generic;

namespace Los_Patitos.Business
{
    public interface ISinpeService
    {
        Task<(bool ok, string error, int? idSinpe)> RegistrarAsync(SinpeModel input);
        Task<List<SinpeModel>> ListarPorCajaAsync(int idCaja);

        Task<bool> SincronizarAsync(int idSinpe);

    }
}