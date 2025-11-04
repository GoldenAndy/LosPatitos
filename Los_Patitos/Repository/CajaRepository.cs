using System.Threading.Tasks;
using Los_Patitos.Data;
using Los_Patitos.Models;
using Microsoft.EntityFrameworkCore;

namespace Los_Patitos.Repositories
{
    public class CajaRepository : ICajaRepository
    {
        private readonly AppDbContext _db;
        public CajaRepository(AppDbContext db) => _db = db;

        public async Task<CajaModel?> ObtenerPorTelefonoAsync(string telefonoSinpe)
        {
            return await _db.Caja_G4
                .FirstOrDefaultAsync(c => c.TelefonoSINPE == telefonoSinpe);
        }
    }
}
