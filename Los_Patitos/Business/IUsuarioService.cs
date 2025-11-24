using System.Threading.Tasks;
using System.Collections.Generic;
using Los_Patitos.Models;

namespace Los_Patitos.Business
{
    public interface IUsuarioService
    {
        Task<(bool ok, string? error, int? idUsuario)> RegistrarAsync(UsuarioModel input);

        Task<(bool ok, string? error)> EditarAsync(UsuarioModel input);

        Task<List<UsuarioModel>> ListarPorComercioAsync(int idComercio);
    }
}