using Los_Patitos.Data;
using Los_Patitos.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Los_Patitos.Business
{
    public interface ICajaService
    {
        Task<IEnumerable<CajaModel>> GetByComercioAsync(int idComercio);
        Task<CajaModel> GetByIdAsync(int id);
        Task<string> CreateAsync(CajaModel caja);
        Task<bool> UpdateAsync(CajaModel caja);
        Task<bool> ExistsByNombreAsync(int idComercio, string nombre);
        Task<bool> ExistsByTelefonoAsync(string telefono);
    }
}
