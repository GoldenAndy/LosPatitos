using Los_Patitos.Data;
using Los_Patitos.Models;

namespace Los_Patitos.Repositories
{
    public class TipoIdentificacionRepository : ITipoIdentificacionRepository
    {
        private readonly AppDbContext _db;
        public TipoIdentificacionRepository(AppDbContext db) { _db = db; }

        public List<TipoIdentificacion> Listar()
            => _db.TiposIdentificacion.OrderBy(x => x.Id).ToList();
    }
}
