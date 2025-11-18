using Los_Patitos.Models;

namespace Los_Patitos.Business
{
    public interface IConfiguracionComercioService
    {
        List<ConfiguracionComercio> Listar();
        ConfiguracionComercio Detalle(int idConfiguracion);
        ConfiguracionComercio? ObtenerPorComercio(int idComercio);
        int Registrar(ConfiguracionComercio nuevo);
        void Actualizar(ConfiguracionComercio cambios);
    }
}
