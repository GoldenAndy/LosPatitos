using Los_Patitos.Models;

namespace Los_Patitos.Repositories
{
    public interface IConfiguracionComercioRepository
    {
        List<ConfiguracionComercio> Listar();
        ConfiguracionComercio? Obtener(int idConfiguracion);
        ConfiguracionComercio? ObtenerPorComercio(int idComercio);
        bool ExisteParaComercio(int idComercio, int? excluirId = null);
        int Crear(ConfiguracionComercio nuevo);
        void Actualizar(ConfiguracionComercio entidad);
    }
}
