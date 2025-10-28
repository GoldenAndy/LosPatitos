using Los_Patitos.Data;
using Los_Patitos.Models;

namespace Los_Patitos.Repositories
{
    public class ComercioRepository : IComercioRepository
    {
        private readonly AppDbContext _db;

        public ComercioRepository(AppDbContext db)
        {
            _db = db;
        }

        public List<Comercio> Listar()
        {
            return _db.Comercios
                      .OrderBy(c => c.Nombre)
                      .ToList();
        }

        public Comercio? Obtener(int id)
        {
            return _db.Comercios
                      .FirstOrDefault(c => c.IdComercio == id);
        }

        public bool ExisteIdentificacion(string identificacion, int? excluirId)
        {
            return _db.Comercios.Any(c =>
                c.Identificacion == identificacion &&
                (!excluirId.HasValue || c.IdComercio != excluirId.Value));
        }

        public int Crear(Comercio nuevo)
        {
            _db.Comercios.Add(nuevo);
            _db.SaveChanges();
            return nuevo.IdComercio; // identity generado por MySQL
        }

        public void Actualizar(Comercio entidad)
        {
            _db.Comercios.Update(entidad);
            _db.SaveChanges();
        }
    }
}
