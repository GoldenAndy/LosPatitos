using Los_Patitos.Models;

namespace Los_Patitos.Repositories
{
    public interface IComercioRepository
    {
        List<Comercio> Listar();
        Comercio? Obtener(int id);
        bool ExisteIdentificacion(string identificacion, int? excluirId);

        int Crear(Comercio nuevo);
        void Actualizar(Comercio entidad);
        Task<Comercio?> ObtenerPorIdAsync(int idComercio);
    }
}
