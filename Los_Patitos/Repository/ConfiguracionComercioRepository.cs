using Los_Patitos.Data;
using Los_Patitos.Models;
using Microsoft.EntityFrameworkCore;

namespace Los_Patitos.Repositories
{
    public class ConfiguracionComercioRepository : IConfiguracionComercioRepository
    {
        private readonly AppDbContext _db;

        public ConfiguracionComercioRepository(AppDbContext db)
        {
            _db = db;
        }

        public List<ConfiguracionComercio> Listar()
        {
            return _db.ConfiguracionesComercio
                      .Include(c => c.Comercio)
                      .OrderBy(c => c.Comercio!.Nombre)
                      .ToList();
        }

        public ConfiguracionComercio? Obtener(int idConfiguracion)
        {
            return _db.ConfiguracionesComercio
                      .Include(c => c.Comercio)
                      .FirstOrDefault(c => c.IdConfiguracion == idConfiguracion);
        }

        public ConfiguracionComercio? ObtenerPorComercio(int idComercio)
        {
            return _db.ConfiguracionesComercio
                      .Include(c => c.Comercio)
                      .FirstOrDefault(c => c.IdComercio == idComercio);
        }

        public bool ExisteParaComercio(int idComercio, int? excluirId = null)
        {
            return _db.ConfiguracionesComercio.Any(c =>
                c.IdComercio == idComercio &&
                (!excluirId.HasValue || c.IdConfiguracion != excluirId.Value));
        }

        public int Crear(ConfiguracionComercio nuevo)
        {
            _db.ConfiguracionesComercio.Add(nuevo);
            _db.SaveChanges();
            return nuevo.IdConfiguracion;
        }

        public void Actualizar(ConfiguracionComercio entidad)
        {
            _db.ConfiguracionesComercio.Update(entidad);
            _db.SaveChanges();
        }
    }
}
