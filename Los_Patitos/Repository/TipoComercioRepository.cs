using Los_Patitos.Data;
using Los_Patitos.Models;

namespace Los_Patitos.Repositories
{
    public class TipoComercioRepository : ITipoComercioRepository
    {
        private readonly AppDbContext _db;
        public TipoComercioRepository(AppDbContext db) { _db = db; }

        public List<TipoComercio> Listar()
            => _db.TiposComercio.OrderBy(x => x.Id).ToList();
    }
}
