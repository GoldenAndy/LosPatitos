using Los_Patitos.Models;
using Los_Patitos.Repositories;

namespace Los_Patitos.Business
{
    public class ComercioService : IComercioService
    {
        private readonly IComercioRepository _repo;
        public ComercioService(IComercioRepository repo) { _repo = repo; }

        public List<Comercio> Listar() => _repo.Listar();

        public Comercio Detalle(int id)
            => _repo.Obtener(id) ?? throw new KeyNotFoundException("Comercio no encontrado.");

        public int Registrar(Comercio nuevo)
        {
            nuevo.Estado = true;
            nuevo.FechaDeRegistro = DateTime.Now;
            nuevo.FechaDeModificacion = null;
            return _repo.Crear(nuevo);
        }

        public void Actualizar(Comercio cambios)
        {
            var existente = _repo.Obtener(cambios.IdComercio)
                            ?? throw new KeyNotFoundException("Comercio no encontrado.");

            existente.Nombre = cambios.Nombre;
            existente.TipoComercio = cambios.TipoComercio;
            existente.Telefono = cambios.Telefono;
            existente.CorreoElectronico = cambios.CorreoElectronico;
            existente.Direccion = cambios.Direccion;
            existente.Estado = cambios.Estado;
            existente.FechaDeModificacion = DateTime.Now;

            _repo.Actualizar(existente);
        }
    }
}
