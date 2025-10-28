using Los_Patitos.Models;
using Los_Patitos.Repositories;

namespace Los_Patitos.Business
{
    public class TipoIdentificacionService : ITipoIdentificacionService
    {
        private readonly ITipoIdentificacionRepository _repo;
        public TipoIdentificacionService(ITipoIdentificacionRepository repo) { _repo = repo; }
        public List<TipoIdentificacion> Listar() => _repo.Listar();
    }
}
