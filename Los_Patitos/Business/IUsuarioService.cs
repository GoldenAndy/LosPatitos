using Los_Patitos.Models;

namespace Los_Patitos.Business
{
    public interface IUsuarioService
    {
        // Registrar
        Task<(bool ok, string? error, int? idUsuario)> RegistrarAsync(UsuarioModel input);

        // Editar
        Task<(bool ok, string? error)> EditarAsync(UsuarioModel input);

        // Listar usuarios por comercio
        Task<List<UsuarioModel>> ListarPorComercioAsync(int idComercio);

        // usuario por Id (para el GET Editar)
        Task<UsuarioModel?> ObtenerPorIdAsync(int idUsuario);
    }
}