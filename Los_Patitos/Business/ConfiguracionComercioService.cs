using Los_Patitos.Models;
using Los_Patitos.Repositories;

namespace Los_Patitos.Business
{
    public class ConfiguracionComercioService : IConfiguracionComercioService
    {
        private readonly IConfiguracionComercioRepository _repo;
        private readonly IComercioRepository _comercios;

        public ConfiguracionComercioService(
            IConfiguracionComercioRepository repo,
            IComercioRepository comercios)
        {
            _repo = repo;
            _comercios = comercios;
        }

        public List<ConfiguracionComercio> Listar() => _repo.Listar();

        public ConfiguracionComercio Detalle(int idConfiguracion)
            => _repo.Obtener(idConfiguracion)
               ?? throw new KeyNotFoundException("Configuración no encontrada.");

        public ConfiguracionComercio? ObtenerPorComercio(int idComercio)
            => _repo.ObtenerPorComercio(idComercio);

        public int Registrar(ConfiguracionComercio nuevo)
        {
            if (_comercios.Obtener(nuevo.IdComercio) is null)
                throw new KeyNotFoundException("El comercio indicado no existe.");

            if (_repo.ExisteParaComercio(nuevo.IdComercio))
                throw new InvalidOperationException("Ya existe una configuración para este comercio.");

            nuevo.FechaDeRegistro = DateTime.Now;
            nuevo.FechaDeModificacion = null;
            nuevo.Estado = true;

            return _repo.Crear(nuevo);
        }

        public void Actualizar(ConfiguracionComercio cambios)
        {
            var existente = _repo.Obtener(cambios.IdConfiguracion)
                           ?? throw new KeyNotFoundException("Configuración no encontrada.");

            existente.TipoConfiguracion = cambios.TipoConfiguracion;
            existente.Comision = cambios.Comision;
            existente.Estado = cambios.Estado;
            existente.FechaDeModificacion = DateTime.Now;

            _repo.Actualizar(existente);
        }
    }
}
