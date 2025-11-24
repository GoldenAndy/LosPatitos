using System.Collections.Generic;
using System.Threading.Tasks;
using Los_Patitos.Models;

namespace Los_Patitos.Repositories
{
    public interface IUsuarioRepository
    {
        Task<int> CrearAsync(UsuarioModel nuevo); // Retorna el ID del usuario
        Task EditarAsync(UsuarioModel usuario); // Actualiza datos del usuario

        Task<UsuarioModel?> ObtenerPorIdAsync(int idUsuario); // Para Editar GET
        Task<UsuarioModel?> ObtenerPorIdentificacionAsync(string identificacion); // Para validar duplicados

        Task<List<UsuarioModel>> ListarPorComercioAsync(int idComercio); // Lista usuarios por comercio
    }
}