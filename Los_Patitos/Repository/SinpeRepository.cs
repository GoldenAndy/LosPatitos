using System.Threading.Tasks;
using Los_Patitos.Data;
using Los_Patitos.Models;
using Microsoft.EntityFrameworkCore;

namespace Los_Patitos.Repositories
{
    public class SinpeRepository : ISinpeRepository
    {
        private readonly AppDbContext _db;
        public SinpeRepository(AppDbContext db) => _db = db;

        public async Task<int> CrearAsync(SinpeModel nuevo)
        {
            await _db.Sinpe_G4.AddAsync(nuevo);
            await _db.SaveChangesAsync();
            return nuevo.IdSinpe;
        }


        public async Task<List<SinpeModel>> ListarPorCajaAsync(int idCaja)
        {
            return await _db.Sinpe_G4
                .Include(s => s.Caja)
                .Where(s => s.IdCaja == idCaja)
                .OrderByDescending(s => s.FechaDeRegistro)
                .Include(s => s.Caja)
                .ToListAsync();
        }


        public async Task<List<SinpeModel>> ListarPorCajasYMesAsync(
            IEnumerable<int> idsCajas,
            int year,
            int month)
                {
                    var ids = idsCajas.ToList();
                    if (!ids.Any())
                        return new List<SinpeModel>();

                    return await _db.Sinpe_G4
                        .Where(s => ids.Contains(s.IdCaja) &&
                                    s.FechaDeRegistro.Year == year &&
                                    s.FechaDeRegistro.Month == month)
                        .ToListAsync();
                }


        //Sincronizar Sinpe
        public async Task<bool> SincronizarAsync(int idSinpe)
        {
            var sinpe = await _db.Sinpe_G4.FindAsync(idSinpe);
            if (sinpe == null) return false;

            sinpe.Estado = true;

            await _db.SaveChangesAsync();
            return true;
        }



    }
}
