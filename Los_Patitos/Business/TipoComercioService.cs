using Los_Patitos.Models;
using Los_Patitos.Repositories;

namespace Los_Patitos.Business
{
    public class TipoComercioService : ITipoComercioService
    {
        private readonly ITipoComercioRepository _repo;
        public TipoComercioService(ITipoComercioRepository repo) { _repo = repo; }
        public List<TipoComercio> Listar() => _repo.Listar();
    }
}
