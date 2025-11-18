using System.Threading.Tasks;
using Los_Patitos.Models;

namespace Los_Patitos.Repositories
{
    public interface ISinpeRepository
    {
        Task<int> CrearAsync(SinpeModel nuevo); //retorna el id del SINPE
        Task<List<SinpeModel>> ListarPorCajaAsync(int idCaja); //Lista los SINPES por id de Caja

        Task<List<SinpeModel>> ListarPorCajasYMesAsync(IEnumerable<int> idsCajas, int year, int month);


    }
}
