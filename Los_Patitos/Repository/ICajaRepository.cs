using System.Threading.Tasks;
using Los_Patitos.Models;

namespace Los_Patitos.Repositories
{
    public interface ICajaRepository
    {
        Task<CajaModel?> ObtenerPorTelefonoAsync(string telefonoSinpe);
    }
}
